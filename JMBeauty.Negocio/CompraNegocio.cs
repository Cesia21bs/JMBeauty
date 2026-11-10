using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Negocio;

public sealed class CompraNegocio
{
    private static void Validar(Compra compra)
    {
        ArgumentNullException.ThrowIfNull(compra);

        if (compra.ProveedorId <= 0)
            throw new ArgumentException("Debe seleccionar un proveedor válido.");

        if (compra.EmpleadoId <= 0)
            throw new ArgumentException("Debe seleccionar el empleado que recibe la compra.");

        if (string.IsNullOrWhiteSpace(compra.NumeroFacturaProveedor))
            throw new ArgumentException("El número de factura del proveedor es requerido.");

        if (compra.Detalles.Count == 0)
            throw new ArgumentException("Debe ingresar al menos un producto en el detalle de la compra.");

        foreach (var d in compra.Detalles)
        {
            if (d.ProductoId <= 0)
                throw new ArgumentException("Producto inválido en el detalle de compra.");
            if (d.Cantidad <= 0)
                throw new ArgumentException("La cantidad comprada debe ser mayor a 0.");
            if (d.CostoUnitario < 0)
                throw new ArgumentException("El costo unitario no puede ser negativo.");
        }
    }

    public static async Task<List<Proveedor>> ListarProveedoresAsync(string clave, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new CompraDatos(conexion);
        return await dal.ListarProveedoresAsync(cancellationToken);
    }

    public static async Task<int> RegistrarCompraAsync(string clave, Compra compra, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        Validar(compra);

        using var conexion = new Conexion(clave);
        var dal = new CompraDatos(conexion);
        return await dal.RegistrarCompraTransaccionalAsync(compra, cancellationToken);
    }
}

