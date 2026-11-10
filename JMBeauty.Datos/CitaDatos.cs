using System.Data;
using JMBeauty.Entidades;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class CitaDatos
{
    private readonly Conexion _conexion;

    public CitaDatos(Conexion conexion)
    {
        _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
    }

    public async Task<List<Cita>> ListarPorRangoAsync(DateTime inicio, DateTime fin, int? empleadoId = null, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT cita_id, fecha_hora_inicio, fecha_hora_fin, estado, observaciones,
                   cliente_id, cliente_nombre, cliente_telefono,
                   empleado_id, estilista_nombre, color_agenda, servicios_resumen, total_estimado
            FROM public.vw_agenda_citas_completa
            WHERE fecha_hora_inicio >= @ini AND fecha_hora_inicio <= @fin
            """;

        if (empleadoId.HasValue && empleadoId.Value > 0)
            sql += " AND empleado_id = @emp";

        sql += " ORDER BY fecha_hora_inicio;";

        var lista = new List<Cita>();
        await using var cmd = _conexion.CrearComando(sql);
        cmd.Parameters.AddWithValue("@ini", inicio.ToUniversalTime());
        cmd.Parameters.AddWithValue("@fin", fin.ToUniversalTime());
        if (empleadoId.HasValue && empleadoId.Value > 0)
            cmd.Parameters.AddWithValue("@emp", empleadoId.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new Cita
            {
                CitaId = reader.GetInt32(0),
                FechaHoraInicio = reader.GetDateTime(1).ToLocalTime(),
                FechaHoraFin = reader.GetDateTime(2).ToLocalTime(),
                Estado = Enum.Parse<EstadoCita>(reader.GetString(3)),
                Observaciones = reader.IsDBNull(4) ? null : reader.GetString(4),
                ClienteId = reader.GetInt32(5),
                ClienteNombre = reader.GetString(6),
                ClienteTelefono = reader.GetString(7),
                EmpleadoId = reader.GetInt32(8),
                EmpleadoNombre = reader.GetString(9),
                ColorAgenda = reader.GetString(10),
                ServiciosResumen = reader.GetString(11),
                TotalEstimado = reader.GetDecimal(12)
            });
        }

        return lista;
    }

    public async Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime inicio, DateTime fin, int? citaIdExcluir = null, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT COUNT(1)
            FROM public.cita
            WHERE empleado_id = @emp
              AND estado NOT IN ('CANCELADA', 'NO_ASISTIO')
              AND (@ini < fecha_hora_fin AND @fin > fecha_hora_inicio)
            """;

        if (citaIdExcluir.HasValue && citaIdExcluir.Value > 0)
            sql += " AND cita_id <> @excluir";

        await using var cmd = _conexion.CrearComando(sql);
        cmd.Parameters.AddWithValue("@emp", empleadoId);
        cmd.Parameters.AddWithValue("@ini", inicio.ToUniversalTime());
        cmd.Parameters.AddWithValue("@fin", fin.ToUniversalTime());
        if (citaIdExcluir.HasValue && citaIdExcluir.Value > 0)
            cmd.Parameters.AddWithValue("@excluir", citaIdExcluir.Value);

        var conteo = Convert.ToInt64(await cmd.ExecuteScalarAsync(cancellationToken));
        return conteo > 0;
    }

    public async Task<int> GuardarCitaCompletaAsync(Cita cita, CancellationToken cancellationToken = default)
    {
        await using var conn = await _conexion.AbrirConexionAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            int citaId;
            if (cita.CitaId == 0)
            {
                const string sqlCita = """
                    INSERT INTO public.cita (cliente_id, empleado_id, fecha_hora_inicio, fecha_hora_fin, estado, observaciones)
                    VALUES (@cli, @emp, @ini, @fin, @est::estado_cita, @obs)
                    RETURNING cita_id;
                    """;

                await using var cmdCita = new NpgsqlCommand(sqlCita, conn, tx);
                cmdCita.Parameters.AddWithValue("@cli", cita.ClienteId);
                cmdCita.Parameters.AddWithValue("@emp", cita.EmpleadoId);
                cmdCita.Parameters.AddWithValue("@ini", cita.FechaHoraInicio.ToUniversalTime());
                cmdCita.Parameters.AddWithValue("@fin", cita.FechaHoraFin.ToUniversalTime());
                cmdCita.Parameters.AddWithValue("@est", cita.Estado.ToString());
                cmdCita.Parameters.AddWithValue("@obs", (object?)cita.Observaciones ?? DBNull.Value);

                citaId = Convert.ToInt32(await cmdCita.ExecuteScalarAsync(cancellationToken));
            }
            else
            {
                citaId = cita.CitaId;
                const string sqlUpdate = """
                    UPDATE public.cita
                    SET cliente_id = @cli,
                        empleado_id = @emp,
                        fecha_hora_inicio = @ini,
                        fecha_hora_fin = @fin,
                        estado = @est::estado_cita,
                        observaciones = @obs
                    WHERE cita_id = @id;
                    """;

                await using var cmdUpdate = new NpgsqlCommand(sqlUpdate, conn, tx);
                cmdUpdate.Parameters.AddWithValue("@id", cita.CitaId);
                cmdUpdate.Parameters.AddWithValue("@cli", cita.ClienteId);
                cmdUpdate.Parameters.AddWithValue("@emp", cita.EmpleadoId);
                cmdUpdate.Parameters.AddWithValue("@ini", cita.FechaHoraInicio.ToUniversalTime());
                cmdUpdate.Parameters.AddWithValue("@fin", cita.FechaHoraFin.ToUniversalTime());
                cmdUpdate.Parameters.AddWithValue("@est", cita.Estado.ToString());
                cmdUpdate.Parameters.AddWithValue("@obs", (object?)cita.Observaciones ?? DBNull.Value);
                await cmdUpdate.ExecuteNonQueryAsync(cancellationToken);

                // Limpiar servicios anteriores para reinsertar
                await using var cmdDel = new NpgsqlCommand("DELETE FROM public.detalle_cita_servicio WHERE cita_id = @id;", conn, tx);
                cmdDel.Parameters.AddWithValue("@id", citaId);
                await cmdDel.ExecuteNonQueryAsync(cancellationToken);
            }

            // Insertar servicios asignados a la cita
            const string sqlDetalle = """
                INSERT INTO public.detalle_cita_servicio (cita_id, servicio_id, duracion_minutos, precio_estimado)
                VALUES (@cid, @sid, @dur, @pre);
                """;

            foreach (var srv in cita.Servicios)
            {
                await using var cmdSrv = new NpgsqlCommand(sqlDetalle, conn, tx);
                cmdSrv.Parameters.AddWithValue("@cid", citaId);
                cmdSrv.Parameters.AddWithValue("@sid", srv.ServicioId);
                cmdSrv.Parameters.AddWithValue("@dur", srv.DuracionMinutos);
                cmdSrv.Parameters.AddWithValue("@pre", srv.PrecioEstimado);
                await cmdSrv.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return citaId;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task CambiarEstadoAsync(int citaId, EstadoCita nuevoEstado, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE public.cita SET estado = @est::estado_cita WHERE cita_id = @id;";
        await using var cmd = _conexion.CrearComando(sql);
        cmd.Parameters.AddWithValue("@id", citaId);
        cmd.Parameters.AddWithValue("@est", nuevoEstado.ToString());
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}

