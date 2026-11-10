using System.Data;
using JMBeauty.Entidades;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class VentaDatos
{
    private readonly Conexion _conexion;

    public VentaDatos(Conexion conexion)
    {
        _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
    }

    public async Task<string> GenerarNumeroComprobanteAsync(CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefijo = $"FAC-{hoy}-";

        const string sql = """
            SELECT COALESCE(MAX(SUBSTRING(numero_comprobante FROM 14 FOR 4)::int), 0) + 1
            FROM public.venta
            WHERE numero_comprobante LIKE @pref;
            """;

        await using var cmd = _conexion.CrearComando(sql);
        cmd.Parameters.AddWithValue("@pref", $"{prefijo}%");
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var correlativo = Convert.ToInt32(result);

        return $"{prefijo}{correlativo:D4}";
    }

    public async Task<int> RegistrarVentaTransaccionalAsync(Venta venta, CancellationToken cancellationToken = default)
    {
        await using var conn = await _conexion.AbrirConexionAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Insertar Cabecera de Venta
            const string sqlVenta = """
                INSERT INTO public.venta (
                    numero_comprobante, cliente_id, empleado_cajero_id, cita_id,
                    fecha_venta, tipo_pago, estado, porcentaje_descuento,
                    monto_descuento, monto_impuesto, monto_total
                )
                VALUES (
                    @num, @cli, @caj, @cit,
                    @fec, @tp::tipo_pago, @est::estado_documento, @pdesc,
                    @mdesc, @mimp, @mtot
                )
                RETURNING venta_id;
                """;

            await using var cmdVenta = new NpgsqlCommand(sqlVenta, conn, tx);
            cmdVenta.Parameters.AddWithValue("@num", venta.NumeroComprobante);
            cmdVenta.Parameters.AddWithValue("@cli", venta.ClienteId);
            cmdVenta.Parameters.AddWithValue("@caj", venta.EmpleadoCajeroId);
            cmdVenta.Parameters.AddWithValue("@cit", (object?)venta.CitaId ?? DBNull.Value);
            cmdVenta.Parameters.AddWithValue("@fec", DateTime.UtcNow);
            cmdVenta.Parameters.AddWithValue("@tp", venta.TipoPago.ToString());
            cmdVenta.Parameters.AddWithValue("@est", venta.Estado.ToString());
            cmdVenta.Parameters.AddWithValue("@pdesc", venta.PorcentajeDescuento);
            cmdVenta.Parameters.AddWithValue("@mdesc", venta.MontoDescuento);
            cmdVenta.Parameters.AddWithValue("@mimp", venta.MontoImpuesto);
            cmdVenta.Parameters.AddWithValue("@mtot", venta.MontoTotal);

            var ventaId = Convert.ToInt32(await cmdVenta.ExecuteScalarAsync(cancellationToken));

            // 2. Insertar Detalle de Servicios
            const string sqlSrv = """
                INSERT INTO public.detalle_venta_servicio (
                    venta_id, servicio_id, empleado_profesional_id, cantidad, precio_unitario, porcentaje_comision
                )
                VALUES (@vid, @sid, @epid, @cant, @pre, @com);
                """;

            foreach (var s in venta.Servicios)
            {
                await using var cmdS = new NpgsqlCommand(sqlSrv, conn, tx);
                cmdS.Parameters.AddWithValue("@vid", ventaId);
                cmdS.Parameters.AddWithValue("@sid", s.ServicioId);
                cmdS.Parameters.AddWithValue("@epid", s.EmpleadoProfesionalId);
                cmdS.Parameters.AddWithValue("@cant", s.Cantidad);
                cmdS.Parameters.AddWithValue("@pre", s.PrecioUnitario);
                cmdS.Parameters.AddWithValue("@com", s.PorcentajeComision);
                await cmdS.ExecuteNonQueryAsync(cancellationToken);
            }

            // 3. Insertar Detalle de Productos
            const string sqlPrd = """
                INSERT INTO public.detalle_venta_producto (
                    venta_id, producto_id, empleado_vendedor_id, cantidad, precio_unitario, porcentaje_comision
                )
                VALUES (@vid, @pid, @evid, @cant, @pre, @com);
                """;

            foreach (var p in venta.Productos)
            {
                await using var cmdP = new NpgsqlCommand(sqlPrd, conn, tx);
                cmdP.Parameters.AddWithValue("@vid", ventaId);
                cmdP.Parameters.AddWithValue("@pid", p.ProductoId);
                cmdP.Parameters.AddWithValue("@evid", p.EmpleadoVendedorId);
                cmdP.Parameters.AddWithValue("@cant", p.Cantidad);
                cmdP.Parameters.AddWithValue("@pre", p.PrecioUnitario);
                cmdP.Parameters.AddWithValue("@com", p.PorcentajeComision);
                await cmdP.ExecuteNonQueryAsync(cancellationToken);
            }

            // 4. Si la venta proviene de una cita, actualizar estado de la cita a FINALIZADA
            if (venta.CitaId.HasValue && venta.CitaId.Value > 0)
            {
                const string sqlCita = "UPDATE public.cita SET estado = 'FINALIZADA' WHERE cita_id = @cid;";
                await using var cmdC = new NpgsqlCommand(sqlCita, conn, tx);
                cmdC.Parameters.AddWithValue("@cid", venta.CitaId.Value);
                await cmdC.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return ventaId;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<List<ComisionResumen>> ListarComisionesAsync(DateOnly? fecha = null, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT empleado_id, profesional, fecha, comision_servicios, comision_productos, total_comision
            FROM public.vw_resumen_comisiones_diarias
            """;

        if (fecha.HasValue)
            sql += " WHERE fecha = @fec";

        sql += " ORDER BY fecha DESC, profesional;";

        var lista = new List<ComisionResumen>();
        await using var cmd = _conexion.CrearComando(sql);
        if (fecha.HasValue)
            cmd.Parameters.AddWithValue("@fec", fecha.Value.ToDateTime(TimeOnly.MinValue));

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var empId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            var profesional = reader.IsDBNull(1) ? "Sin nombre" : reader.GetString(1);
            var fechaComision = reader.IsDBNull(2) ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.FromDateTime(reader.GetDateTime(2));
            var comServicios = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
            var comProductos = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
            var totalCom = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5);

            lista.Add(new ComisionResumen
            {
                EmpleadoId = empId,
                Profesional = profesional,
                Fecha = fechaComision,
                ComisionServicios = comServicios,
                ComisionProductos = comProductos,
                TotalComision = totalCom
            });
        }

        return lista;
    }
}

