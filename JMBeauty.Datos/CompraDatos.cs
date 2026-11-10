using System.Data;
using JMBeauty.Entidades;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class CompraDatos
{
    private readonly Conexion _conexion;

    public CompraDatos(Conexion conexion)
    {
        _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
    }

    public async Task<List<Proveedor>> ListarProveedoresAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT proveedor_id, razon_social, contacto, telefono, direccion, activo FROM public.proveedor ORDER BY razon_social;";
        var lista = new List<Proveedor>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new Proveedor
            {
                ProveedorId = reader.GetInt32(0),
                RazonSocial = reader.GetString(1),
                Contacto = reader.IsDBNull(2) ? null : reader.GetString(2),
                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                Direccion = reader.IsDBNull(4) ? null : reader.GetString(4),
                Activo = reader.GetBoolean(5)
            });
        }
        return lista;
    }

    public async Task<int> RegistrarCompraTransaccionalAsync(Compra compra, CancellationToken cancellationToken = default)
    {
        await using var conn = await _conexion.AbrirConexionAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            const string sqlCompra = """
                INSERT INTO public.compra (proveedor_id, empleado_id, numero_factura_proveedor, fecha_compra, estado, notas)
                VALUES (@prov, @emp, @num, @fec, @est::estado_documento, @not)
                RETURNING compra_id;
                """;

            await using var cmdCompra = new NpgsqlCommand(sqlCompra, conn, tx);
            cmdCompra.Parameters.AddWithValue("@prov", compra.ProveedorId);
            cmdCompra.Parameters.AddWithValue("@emp", compra.EmpleadoId);
            cmdCompra.Parameters.AddWithValue("@num", compra.NumeroFacturaProveedor);
            cmdCompra.Parameters.AddWithValue("@fec", DateTime.UtcNow);
            cmdCompra.Parameters.AddWithValue("@est", compra.Estado.ToString());
            cmdCompra.Parameters.AddWithValue("@not", (object?)compra.Notas ?? DBNull.Value);

            var compraId = Convert.ToInt32(await cmdCompra.ExecuteScalarAsync(cancellationToken));

            const string sqlDetalle = """
                INSERT INTO public.detalle_compra (compra_id, producto_id, cantidad, costo_unitario)
                VALUES (@cid, @pid, @cant, @costo);
                """;

            foreach (var d in compra.Detalles)
            {
                await using var cmdD = new NpgsqlCommand(sqlDetalle, conn, tx);
                cmdD.Parameters.AddWithValue("@cid", compraId);
                cmdD.Parameters.AddWithValue("@pid", d.ProductoId);
                cmdD.Parameters.AddWithValue("@cant", d.Cantidad);
                cmdD.Parameters.AddWithValue("@costo", d.CostoUnitario);
                await cmdD.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return compraId;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

