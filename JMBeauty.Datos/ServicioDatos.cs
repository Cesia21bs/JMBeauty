using System.Data;
using JMBeauty.Entidades;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class ServicioDatos
{
    private readonly Conexion _conexion;

    public ServicioDatos(Conexion conexion)
    {
        _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
    }

    public async Task<List<CategoriaServicio>> ListarCategoriasAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT categoria_servicio_id, nombre, activo FROM public.categoria_servicio ORDER BY nombre;";
        var lista = new List<CategoriaServicio>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new CategoriaServicio
            {
                CategoriaServicioId = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Activo = reader.GetBoolean(2)
            });
        }
        return lista;
    }

    public async Task<List<Servicio>> ListarAsync(bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT s.servicio_id, s.categoria_servicio_id, cs.nombre AS categoria_nombre,
                   s.codigo, s.nombre, s.descripcion, s.duracion_minutos, s.precio_base, s.activo
            FROM public.servicio s
            JOIN public.categoria_servicio cs ON s.categoria_servicio_id = cs.categoria_servicio_id
            """ + (soloActivos ? " WHERE s.activo = TRUE" : "") + " ORDER BY cs.nombre, s.nombre;";

        var lista = new List<Servicio>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new Servicio
            {
                ServicioId = reader.GetInt32(0),
                CategoriaServicioId = reader.GetInt32(1),
                CategoriaNombre = reader.GetString(2),
                Codigo = reader.GetString(3),
                Nombre = reader.GetString(4),
                Descripcion = reader.IsDBNull(5) ? null : reader.GetString(5),
                DuracionMinutos = reader.GetInt32(6),
                PrecioBase = reader.GetDecimal(7),
                Activo = reader.GetBoolean(8)
            });
        }
        return lista;
    }

    public async Task<int> GuardarAsync(Servicio s, Servicio? original, CancellationToken cancellationToken = default)
    {
        if (original is null)
        {
            const string sqlInsert = """
                INSERT INTO public.servicio (categoria_servicio_id, codigo, nombre, descripcion, duracion_minutos, precio_base, activo)
                VALUES (@cat, @cod, @nom, @des, @dur, @pre, @act)
                RETURNING servicio_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlInsert);
            cmd.Parameters.AddWithValue("@cat", s.CategoriaServicioId);
            cmd.Parameters.AddWithValue("@cod", s.Codigo);
            cmd.Parameters.AddWithValue("@nom", s.Nombre);
            cmd.Parameters.AddWithValue("@des", (object?)s.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dur", s.DuracionMinutos);
            cmd.Parameters.AddWithValue("@pre", s.PrecioBase);
            cmd.Parameters.AddWithValue("@act", s.Activo);

            try
            {
                var id = await cmd.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt32(id);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Ya existe un servicio con el código ingresado.", ex);
            }
        }
        else
        {
            const string sqlUpdate = """
                UPDATE public.servicio
                SET categoria_servicio_id = @cat,
                    codigo = @cod,
                    nombre = @nom,
                    descripcion = @des,
                    duracion_minutos = @dur,
                    precio_base = @pre,
                    activo = @act
                WHERE servicio_id = @id
                  AND codigo = @ocod
                  AND nombre = @onom
                  AND duracion_minutos = @odur
                  AND precio_base = @opre
                  AND activo = @oact
                RETURNING servicio_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlUpdate);
            cmd.Parameters.AddWithValue("@id", s.ServicioId);
            cmd.Parameters.AddWithValue("@cat", s.CategoriaServicioId);
            cmd.Parameters.AddWithValue("@cod", s.Codigo);
            cmd.Parameters.AddWithValue("@nom", s.Nombre);
            cmd.Parameters.AddWithValue("@des", (object?)s.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dur", s.DuracionMinutos);
            cmd.Parameters.AddWithValue("@pre", s.PrecioBase);
            cmd.Parameters.AddWithValue("@act", s.Activo);

            // Concurrencia
            cmd.Parameters.AddWithValue("@ocod", original.Codigo);
            cmd.Parameters.AddWithValue("@onom", original.Nombre);
            cmd.Parameters.AddWithValue("@odur", original.DuracionMinutos);
            cmd.Parameters.AddWithValue("@opre", original.PrecioBase);
            cmd.Parameters.AddWithValue("@oact", original.Activo);

            try
            {
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                if (result is null)
                {
                    throw new InvalidOperationException("El servicio fue modificado por otro operador. Recargue los datos.");
                }
                return s.ServicioId;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Ya existe un servicio con el código ingresado.", ex);
            }
        }
    }
}

