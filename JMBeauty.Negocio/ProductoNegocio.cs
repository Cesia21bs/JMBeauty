using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Negocio;

public sealed class ProductoNegocio
{
    private static void Validar(Producto p)
    {
        ArgumentNullException.ThrowIfNull(p);

        if (p.CategoriaProductoId <= 0)
            throw new ArgumentException("Debe seleccionar una categoría de producto válida.");

        if (string.IsNullOrWhiteSpace(p.Codigo) || p.Codigo.Trim().Length > 30)
            throw new ArgumentException("El código del producto es obligatorio (máximo 30 caracteres).");

        if (string.IsNullOrWhiteSpace(p.Nombre) || p.Nombre.Trim().Length > 150)
            throw new ArgumentException("El nombre del producto es obligatorio (máximo 150 caracteres).");

        if (p.PrecioCosto < 0)
            throw new ArgumentException("El precio de costo no puede ser negativo.");

        if (p.PrecioVenta < 0)
            throw new ArgumentException("El precio de venta no puede ser negativo.");

        if (p.ExistenciaMinima < 0)
            throw new ArgumentException("La existencia mínima no puede ser negativa.");
    }

    public static async Task<List<CategoriaProducto>> ListarCategoriasAsync(string clave, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new ProductoDatos(conexion);
        return await dal.ListarCategoriasAsync(cancellationToken);
    }

    public static async Task<List<Producto>> ListarAsync(string clave, bool soloActivos = false, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new ProductoDatos(conexion);
        return await dal.ListarAsync(soloActivos, cancellationToken);
    }

    public static async Task<List<InventarioItem>> ListarInventarioActualAsync(string clave, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new ProductoDatos(conexion);
        return await dal.ListarInventarioActualAsync(cancellationToken);
    }

    public static async Task<int> GuardarAsync(string clave, Producto p, Producto? original, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        Validar(p);

        if (original is not null && p.ProductoId != original.ProductoId)
            throw new InvalidOperationException("Incoherencia de identidad en la edición del producto.");

        using var conexion = new Conexion(clave);
        var dal = new ProductoDatos(conexion);
        return await dal.GuardarAsync(p, original, cancellationToken);
    }
}

