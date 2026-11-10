using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Negocio;

public sealed class EmpleadoNegocio
{
    private static void Validar(Empleado emp)
    {
        ArgumentNullException.ThrowIfNull(emp);

        if (string.IsNullOrWhiteSpace(emp.Nombres) || emp.Nombres.Trim().Length > 80)
            throw new ArgumentException("Los nombres del empleado son obligatorios (máximo 80 caracteres).");

        if (emp.Nombres.Any(char.IsDigit))
            throw new ArgumentException("Los nombres del empleado no pueden contener números.");

        if (string.IsNullOrWhiteSpace(emp.Apellidos) || emp.Apellidos.Trim().Length > 80)
            throw new ArgumentException("Los apellidos del empleado son obligatorios (máximo 80 caracteres).");

        if (emp.Apellidos.Any(char.IsDigit))
            throw new ArgumentException("Los apellidos del empleado no pueden contener números.");

        if (string.IsNullOrWhiteSpace(emp.DocumentoIdentidad) || emp.DocumentoIdentidad.Trim().Length > 30)
            throw new ArgumentException("El documento de identidad es obligatorio (máximo 30 caracteres).");

        if (!emp.DocumentoIdentidad.All(c => char.IsDigit(c) || c == '-' || char.IsWhiteSpace(c)))
            throw new ArgumentException("El documento de identidad solo puede contener números y guiones.");

        if (string.IsNullOrWhiteSpace(emp.Telefono) || emp.Telefono.Trim().Length > 25)
            throw new ArgumentException("El teléfono es obligatorio (máximo 25 caracteres).");

        if (!emp.Telefono.All(c => char.IsDigit(c) || c == '-' || c == '+' || char.IsWhiteSpace(c)))
            throw new ArgumentException("El teléfono solo puede contener números y guiones.");

        if (emp.RolId <= 0)
            throw new ArgumentException("Debe seleccionar un rol válido para el empleado.");

        if (emp.PorcentajeComisionServicio is < 0 or > 100)
            throw new ArgumentException("El porcentaje de comisión por servicio debe estar entre 0% y 100%.");

        if (emp.PorcentajeComisionProducto is < 0 or > 100)
            throw new ArgumentException("El porcentaje de comisión por producto debe estar entre 0% y 100%.");
    }

    public static async Task<List<Rol>> ListarRolesAsync(string clave, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new EmpleadoDatos(conexion);
        return await dal.ListarRolesAsync(cancellationToken);
    }

    public static async Task<int> GuardarRolAsync(string clave, Rol rol, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        ArgumentNullException.ThrowIfNull(rol);

        if (string.IsNullOrWhiteSpace(rol.Nombre) || rol.Nombre.Trim().Length > 50)
            throw new ArgumentException("El nombre del rol es obligatorio (máximo 50 caracteres).");

        using var conexion = new Conexion(clave);
        var dal = new EmpleadoDatos(conexion);
        return await dal.GuardarRolAsync(rol, cancellationToken);
    }

    public static async Task<Empleado?> AutenticarAsync(string claveBd, string usuario, string password, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(claveBd);
        if (string.IsNullOrWhiteSpace(usuario))
            throw new ArgumentException("Debe ingresar su documento de identidad o correo electrónico.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Debe ingresar su contraseña / PIN.");

        using var conexion = new Conexion(claveBd);
        var dal = new EmpleadoDatos(conexion);
        return await dal.AutenticarAsync(usuario, password, cancellationToken);
    }

    public static async Task<List<Empleado>> ListarEmpleadosAsync(string clave, bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new EmpleadoDatos(conexion);
        return await dal.ListarEmpleadosAsync(soloActivos, cancellationToken);
    }

    public static async Task<int> GuardarAsync(string clave, Empleado emp, Empleado? original, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        Validar(emp);

        if (original is not null && emp.EmpleadoId != original.EmpleadoId)
            throw new InvalidOperationException("Incoherencia de identidad en la edición del empleado.");

        using var conexion = new Conexion(clave);
        var dal = new EmpleadoDatos(conexion);
        return await dal.GuardarAsync(emp, original, cancellationToken);
    }
}

