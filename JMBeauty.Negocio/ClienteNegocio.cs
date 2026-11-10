using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Negocio;

public sealed class ClienteNegocio
{
    private static void Validar(Cliente cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        if (string.IsNullOrWhiteSpace(cliente.Nombres) || cliente.Nombres.Trim().Length > 80)
            throw new ArgumentException("Los nombres del cliente son requeridos (máximo 80 caracteres).");

        if (cliente.Nombres.Any(char.IsDigit))
            throw new ArgumentException("Los nombres del cliente no pueden contener números.");

        if (string.IsNullOrWhiteSpace(cliente.Apellidos) || cliente.Apellidos.Trim().Length > 80)
            throw new ArgumentException("Los apellidos del cliente son requeridos (máximo 80 caracteres).");

        if (cliente.Apellidos.Any(char.IsDigit))
            throw new ArgumentException("Los apellidos del cliente no pueden contener números.");

        if (string.IsNullOrWhiteSpace(cliente.Telefono) || cliente.Telefono.Trim().Length > 25)
            throw new ArgumentException("El teléfono es requerido (máximo 25 caracteres).");

        if (!cliente.Telefono.All(c => char.IsDigit(c) || c == '-' || c == '+' || char.IsWhiteSpace(c)))
            throw new ArgumentException("El teléfono del cliente solo puede contener números y guiones.");
    }

    public static async Task<List<Cliente>> ListarAsync(string clave, string? filtro = null, bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new ClienteDatos(conexion);
        return await dal.ListarAsync(filtro, soloActivos, cancellationToken);
    }

    public static async Task<int> GuardarAsync(string clave, Cliente cliente, Cliente? original, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        Validar(cliente);

        if (original is not null && cliente.ClienteId != original.ClienteId)
            throw new InvalidOperationException("Incoherencia en la clave primaria del cliente a actualizar.");

        using var conexion = new Conexion(clave);
        var dal = new ClienteDatos(conexion);
        return await dal.GuardarAsync(cliente, original, cancellationToken);
    }
}

