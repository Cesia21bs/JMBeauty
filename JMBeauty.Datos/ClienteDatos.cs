using System.Data;
using JMBeauty.Entidades;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class ClienteDatos
{
    private readonly Conexion _conexion;

    public ClienteDatos(Conexion conexion)
    {
        _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
    }

    public async Task<List<Cliente>> ListarAsync(string? filtro = null, bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT cliente_id, nombres, apellidos, telefono, email,
                   direccion, historial_alergias_notas, fecha_nacimiento, activo, fecha_creacion
            FROM public.cliente
            WHERE 1 = 1
            """;

        if (soloActivos)
            sql += " AND activo = TRUE";

        if (!string.IsNullOrWhiteSpace(filtro))
            sql += " AND (lower(nombres) LIKE @f OR lower(apellidos) LIKE @f OR telefono LIKE @f)";

        sql += " ORDER BY apellidos, nombres;";

        var lista = new List<Cliente>();
        await using var cmd = _conexion.CrearComando(sql);
        if (!string.IsNullOrWhiteSpace(filtro))
            cmd.Parameters.AddWithValue("@f", $"%{filtro.Trim().ToLowerInvariant()}%");

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new Cliente
            {
                ClienteId = reader.GetInt32(0),
                Nombres = reader.GetString(1),
                Apellidos = reader.GetString(2),
                Telefono = reader.GetString(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                Direccion = reader.IsDBNull(5) ? null : reader.GetString(5),
                HistorialAlergiasNotas = reader.IsDBNull(6) ? null : reader.GetString(6),
                FechaNacimiento = reader.IsDBNull(7) ? null : DateOnly.FromDateTime(reader.GetDateTime(7)),
                Activo = reader.GetBoolean(8),
                FechaCreacion = reader.GetDateTime(9)
            });
        }

        return lista;
    }

    public async Task<int> GuardarAsync(Cliente cliente, Cliente? original, CancellationToken cancellationToken = default)
    {
        if (original is null)
        {
            const string sqlInsert = """
                INSERT INTO public.cliente (
                    nombres, apellidos, telefono, email, direccion,
                    historial_alergias_notas, fecha_nacimiento, activo
                )
                VALUES (
                    @nom, @ape, @tel, @mail, @dir,
                    @hist, @fec, @act
                )
                RETURNING cliente_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlInsert);
            cmd.Parameters.AddWithValue("@nom", cliente.Nombres);
            cmd.Parameters.AddWithValue("@ape", cliente.Apellidos);
            cmd.Parameters.AddWithValue("@tel", cliente.Telefono);
            cmd.Parameters.AddWithValue("@mail", (object?)cliente.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dir", (object?)cliente.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hist", (object?)cliente.HistorialAlergiasNotas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fec", cliente.FechaNacimiento.HasValue ? cliente.FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            cmd.Parameters.AddWithValue("@act", cliente.Activo);

            var id = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(id);
        }
        else
        {
            const string sqlUpdate = """
                UPDATE public.cliente
                SET nombres = @nom,
                    apellidos = @ape,
                    telefono = @tel,
                    email = @mail,
                    direccion = @dir,
                    historial_alergias_notas = @hist,
                    fecha_nacimiento = @fec,
                    activo = @act
                WHERE cliente_id = @id
                  AND nombres = @onom
                  AND apellidos = @oape
                  AND telefono = @otel
                  AND activo = @oact
                RETURNING cliente_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlUpdate);
            cmd.Parameters.AddWithValue("@id", cliente.ClienteId);
            cmd.Parameters.AddWithValue("@nom", cliente.Nombres);
            cmd.Parameters.AddWithValue("@ape", cliente.Apellidos);
            cmd.Parameters.AddWithValue("@tel", cliente.Telefono);
            cmd.Parameters.AddWithValue("@mail", (object?)cliente.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dir", (object?)cliente.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hist", (object?)cliente.HistorialAlergiasNotas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fec", cliente.FechaNacimiento.HasValue ? cliente.FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            cmd.Parameters.AddWithValue("@act", cliente.Activo);

            // Concurrencia optimista
            cmd.Parameters.AddWithValue("@onom", original.Nombres);
            cmd.Parameters.AddWithValue("@oape", original.Apellidos);
            cmd.Parameters.AddWithValue("@otel", original.Telefono);
            cmd.Parameters.AddWithValue("@oact", original.Activo);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (result is null)
            {
                throw new InvalidOperationException("El cliente fue modificado por otro usuario en la red. Recargue los datos para continuar.");
            }
            return cliente.ClienteId;
        }
    }
}

