using System.Data;
using JMBeauty.Entidades;
using Npgsql;

namespace JMBeauty.Datos;

public sealed class EmpleadoDatos
{
    private readonly Conexion _conexion;

    public EmpleadoDatos(Conexion conexion)
    {
        _conexion = conexion ?? throw new ArgumentNullException(nameof(conexion));
    }

    public async Task<List<Rol>> ListarRolesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT rol_id, nombre, descripcion, tipo_acceso, activo
            FROM public.rol
            ORDER BY nombre;
            """;

        var lista = new List<Rol>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new Rol
            {
                RolId = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                TipoAcceso = reader.IsDBNull(3) ? "ESPECIALISTA" : reader.GetString(3),
                Activo = reader.GetBoolean(4)
            });
        }

        return lista;
    }

    public async Task<int> GuardarRolAsync(Rol rol, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO public.rol (nombre, descripcion, tipo_acceso, activo)
            VALUES (@nom, @des, @tip, @act)
            RETURNING rol_id;
            """;

        await using var cmd = _conexion.CrearComando(sql);
        cmd.Parameters.AddWithValue("@nom", rol.Nombre.Trim());
        cmd.Parameters.AddWithValue("@des", (object?)rol.Descripcion?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tip", string.IsNullOrWhiteSpace(rol.TipoAcceso) ? "ESPECIALISTA" : rol.TipoAcceso.Trim().ToUpperInvariant());
        cmd.Parameters.AddWithValue("@act", rol.Activo);

        try
        {
            var id = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(id);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new InvalidOperationException($"Ya existe un rol registrado con el nombre '{rol.Nombre}'.", ex);
        }
    }

    public async Task<List<Empleado>> ListarEmpleadosAsync(bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT e.empleado_id, e.rol_id, r.nombre AS rol_nombre, r.tipo_acceso, e.nombres, e.apellidos,
                   e.documento_identidad, e.telefono, e.email, e.porcentaje_comision_servicio,
                   e.porcentaje_comision_producto, e.color_agenda, e.activo, e.fecha_creacion, e.clave_acceso
            FROM public.empleado e
            JOIN public.rol r ON e.rol_id = r.rol_id
            """ + (soloActivos ? " WHERE e.activo = TRUE" : "") + " ORDER BY e.nombres, e.apellidos;";

        var lista = new List<Empleado>();
        await using var cmd = _conexion.CrearComando(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new Empleado
            {
                EmpleadoId = reader.GetInt32(0),
                RolId = reader.GetInt32(1),
                RolNombre = reader.GetString(2),
                TipoAcceso = reader.IsDBNull(3) ? "ESPECIALISTA" : reader.GetString(3),
                Nombres = reader.GetString(4),
                Apellidos = reader.GetString(5),
                DocumentoIdentidad = reader.GetString(6),
                Telefono = reader.GetString(7),
                Email = reader.IsDBNull(8) ? null : reader.GetString(8),
                PorcentajeComisionServicio = reader.GetDecimal(9),
                PorcentajeComisionProducto = reader.GetDecimal(10),
                ColorAgenda = reader.GetString(11),
                Activo = reader.GetBoolean(12),
                FechaCreacion = reader.GetDateTime(13),
                ClaveAcceso = reader.IsDBNull(14) ? "1234" : reader.GetString(14)
            });
        }

        return lista;
    }

    public async Task<Empleado?> AutenticarAsync(string documentoOEmail, string clave, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT e.empleado_id, e.rol_id, r.nombre AS rol_nombre, r.tipo_acceso, e.nombres, e.apellidos,
                   e.documento_identidad, e.telefono, e.email, e.porcentaje_comision_servicio,
                   e.porcentaje_comision_producto, e.color_agenda, e.activo, e.fecha_creacion, e.clave_acceso
            FROM public.empleado e
            JOIN public.rol r ON e.rol_id = r.rol_id
            WHERE e.activo = TRUE
              AND (lower(trim(e.documento_identidad)) = lower(trim(@usr)) OR lower(trim(e.email)) = lower(trim(@usr)))
              AND e.clave_acceso = @pwd;
            """;

        await using var cmd = _conexion.CrearComando(sql);
        cmd.Parameters.AddWithValue("@usr", documentoOEmail.Trim());
        cmd.Parameters.AddWithValue("@pwd", clave.Trim());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new Empleado
            {
                EmpleadoId = reader.GetInt32(0),
                RolId = reader.GetInt32(1),
                RolNombre = reader.GetString(2),
                TipoAcceso = reader.IsDBNull(3) ? "ESPECIALISTA" : reader.GetString(3),
                Nombres = reader.GetString(4),
                Apellidos = reader.GetString(5),
                DocumentoIdentidad = reader.GetString(6),
                Telefono = reader.GetString(7),
                Email = reader.IsDBNull(8) ? null : reader.GetString(8),
                PorcentajeComisionServicio = reader.GetDecimal(9),
                PorcentajeComisionProducto = reader.GetDecimal(10),
                ColorAgenda = reader.GetString(11),
                Activo = reader.GetBoolean(12),
                FechaCreacion = reader.GetDateTime(13),
                ClaveAcceso = reader.IsDBNull(14) ? "1234" : reader.GetString(14)
            };
        }

        return null;
    }

    public async Task<int> GuardarAsync(Empleado emp, Empleado? original, CancellationToken cancellationToken = default)
    {
        if (original is null)
        {
            const string sqlInsert = """
                INSERT INTO public.empleado (
                    rol_id, nombres, apellidos, documento_identidad, telefono,
                    email, clave_acceso, porcentaje_comision_servicio, porcentaje_comision_producto,
                    color_agenda, activo
                )
                VALUES (
                    @rol, @nom, @ape, @doc, @tel,
                    @mail, @pwd, @coms, @comp, @color, @act
                )
                RETURNING empleado_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlInsert);
            cmd.Parameters.AddWithValue("@rol", emp.RolId);
            cmd.Parameters.AddWithValue("@nom", emp.Nombres);
            cmd.Parameters.AddWithValue("@ape", emp.Apellidos);
            cmd.Parameters.AddWithValue("@doc", emp.DocumentoIdentidad);
            cmd.Parameters.AddWithValue("@tel", emp.Telefono);
            cmd.Parameters.AddWithValue("@mail", (object?)emp.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pwd", string.IsNullOrWhiteSpace(emp.ClaveAcceso) ? "1234" : emp.ClaveAcceso.Trim());
            cmd.Parameters.AddWithValue("@coms", emp.PorcentajeComisionServicio);
            cmd.Parameters.AddWithValue("@comp", emp.PorcentajeComisionProducto);
            cmd.Parameters.AddWithValue("@color", emp.ColorAgenda);
            cmd.Parameters.AddWithValue("@act", emp.Activo);

            try
            {
                var id = await cmd.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt32(id);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Ya existe un empleado registrado con ese documento de identidad.", ex);
            }
        }
        else
        {
            const string sqlUpdate = """
                UPDATE public.empleado
                SET rol_id = @rol,
                    nombres = @nom,
                    apellidos = @ape,
                    documento_identidad = @doc,
                    telefono = @tel,
                    email = @mail,
                    clave_acceso = @pwd,
                    porcentaje_comision_servicio = @coms,
                    porcentaje_comision_producto = @comp,
                    color_agenda = @color,
                    activo = @act
                WHERE empleado_id = @id
                  AND rol_id = @orol
                  AND nombres = @onom
                  AND apellidos = @oape
                  AND documento_identidad = @odoc
                  AND telefono = @otel
                  AND activo = @oact
                RETURNING empleado_id;
                """;

            await using var cmd = _conexion.CrearComando(sqlUpdate);
            cmd.Parameters.AddWithValue("@id", emp.EmpleadoId);
            cmd.Parameters.AddWithValue("@rol", emp.RolId);
            cmd.Parameters.AddWithValue("@nom", emp.Nombres);
            cmd.Parameters.AddWithValue("@ape", emp.Apellidos);
            cmd.Parameters.AddWithValue("@doc", emp.DocumentoIdentidad);
            cmd.Parameters.AddWithValue("@tel", emp.Telefono);
            cmd.Parameters.AddWithValue("@mail", (object?)emp.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pwd", string.IsNullOrWhiteSpace(emp.ClaveAcceso) ? "1234" : emp.ClaveAcceso.Trim());
            cmd.Parameters.AddWithValue("@coms", emp.PorcentajeComisionServicio);
            cmd.Parameters.AddWithValue("@comp", emp.PorcentajeComisionProducto);
            cmd.Parameters.AddWithValue("@color", emp.ColorAgenda);
            cmd.Parameters.AddWithValue("@act", emp.Activo);

            // Parámetros de concurrencia optimista
            cmd.Parameters.AddWithValue("@orol", original.RolId);
            cmd.Parameters.AddWithValue("@onom", original.Nombres);
            cmd.Parameters.AddWithValue("@oape", original.Apellidos);
            cmd.Parameters.AddWithValue("@odoc", original.DocumentoIdentidad);
            cmd.Parameters.AddWithValue("@otel", original.Telefono);
            cmd.Parameters.AddWithValue("@oact", original.Activo);

            try
            {
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                if (result is null)
                {
                    throw new InvalidOperationException("El empleado fue modificado por otro usuario. Por favor recargue los datos.");
                }
                return emp.EmpleadoId;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException("Ya existe un empleado registrado con ese documento de identidad.", ex);
            }
        }
    }
}

