using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Negocio;

public sealed class VentaNegocio
{
    private static void Validar(Venta venta)
    {
        ArgumentNullException.ThrowIfNull(venta);

        if (venta.ClienteId <= 0)
            throw new ArgumentException("Debe seleccionar un cliente para registrar la venta.");

        if (venta.EmpleadoCajeroId <= 0)
            throw new ArgumentException("Debe asignar un cajero responsable.");

        if (venta.Servicios.Count == 0 && venta.Productos.Count == 0)
            throw new ArgumentException("La venta debe contener al menos un servicio o un producto.");

        if (venta.PorcentajeDescuento is < 0 or > 100)
            throw new ArgumentException("El porcentaje de descuento debe estar entre 0% y 100%.");

        if (venta.MontoTotal < 0)
            throw new ArgumentException("El monto total no puede ser negativo.");
    }

    public static async Task<string> GenerarNumeroComprobanteAsync(string clave, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new VentaDatos(conexion);
        return await dal.GenerarNumeroComprobanteAsync(cancellationToken);
    }

    public static async Task<int> RegistrarVentaAsync(string clave, Venta venta, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        Validar(venta);

        using var conexion = new Conexion(clave);
        var dal = new VentaDatos(conexion);

        if (string.IsNullOrWhiteSpace(venta.NumeroComprobante))
        {
            venta.NumeroComprobante = await dal.GenerarNumeroComprobanteAsync(cancellationToken);
        }

        return await dal.RegistrarVentaTransaccionalAsync(venta, cancellationToken);
    }

    public static async Task<List<ComisionResumen>> ListarComisionesAsync(string clave, DateOnly? fecha = null, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new VentaDatos(conexion);
        return await dal.ListarComisionesAsync(fecha, cancellationToken);
    }
}

