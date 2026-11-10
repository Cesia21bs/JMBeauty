using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Negocio;

public sealed class ServicioNegocio
{
    private static void Validar(Servicio s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (s.CategoriaServicioId <= 0)
            throw new ArgumentException("Debe seleccionar una categoría de servicio válida.");

        if (string.IsNullOrWhiteSpace(s.Codigo) || s.Codigo.Trim().Length > 30)
            throw new ArgumentException("El código del servicio es obligatorio (máximo 30 caracteres).");

        if (string.IsNullOrWhiteSpace(s.Nombre) || s.Nombre.Trim().Length > 120)
            throw new ArgumentException("El nombre del servicio es obligatorio (máximo 120 caracteres).");

        if (s.DuracionMinutos <= 0)
            throw new ArgumentException("La duración estimada en minutos debe ser mayor a 0.");

        if (s.PrecioBase < 0)
            throw new ArgumentException("El precio base no puede ser negativo.");
    }

    public static async Task<List<CategoriaServicio>> ListarCategoriasAsync(string clave, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new ServicioDatos(conexion);
        return await dal.ListarCategoriasAsync(cancellationToken);
    }

    public static async Task<List<Servicio>> ListarAsync(string clave, bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new ServicioDatos(conexion);
        return await dal.ListarAsync(soloActivos, cancellationToken);
    }

    public static async Task<int> GuardarAsync(string clave, Servicio s, Servicio? original, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        Validar(s);

        if (original is not null && s.ServicioId != original.ServicioId)
            throw new InvalidOperationException("Incoherencia al actualizar servicio.");

        using var conexion = new Conexion(clave);
        var dal = new ServicioDatos(conexion);
        return await dal.GuardarAsync(s, original, cancellationToken);
    }
}

