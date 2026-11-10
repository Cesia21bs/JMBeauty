namespace JMBeauty.Entidades;

public sealed class Venta
{
    public int VentaId { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int EmpleadoCajeroId { get; set; }
    public int? CitaId { get; set; }
    public DateTime FechaVenta { get; set; }
    public TipoPago TipoPago { get; set; } = TipoPago.EFECTIVO;
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.EMITIDA;
    public decimal PorcentajeDescuento { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal MontoImpuesto { get; set; }
    public decimal MontoTotal { get; set; }

    // Propiedades de navegación
    public string ClienteNombre { get; set; } = string.Empty;
    public string CajeroNombre { get; set; } = string.Empty;

    public List<DetalleVentaServicio> Servicios { get; set; } = [];
    public List<DetalleVentaProducto> Productos { get; set; } = [];
}

public sealed class DetalleVentaServicio
{
    public int DetalleServicioId { get; set; }
    public int VentaId { get; set; }
    public int ServicioId { get; set; }
    public int EmpleadoProfesionalId { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeComision { get; set; }
    public decimal MontoComision { get; set; }
    public decimal Subtotal { get; set; }

    public string ServicioNombre { get; set; } = string.Empty;
    public string ProfesionalNombre { get; set; } = string.Empty;
}

public sealed class DetalleVentaProducto
{
    public int DetalleProductoId { get; set; }
    public int VentaId { get; set; }
    public int ProductoId { get; set; }
    public int EmpleadoVendedorId { get; set; }
    public decimal Cantidad { get; set; } = 1.00m;
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeComision { get; set; }
    public decimal MontoComision { get; set; }
    public decimal Subtotal { get; set; }

    public string ProductoNombre { get; set; } = string.Empty;
    public string VendedorNombre { get; set; } = string.Empty;
}

