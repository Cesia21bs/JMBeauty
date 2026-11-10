namespace JMBeauty.Entidades;

public sealed class Proveedor
{
    public int ProveedorId { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;

    public string DescripcionCombo => Activo ? RazonSocial : $"{RazonSocial} (inactivo)";
}

public sealed class Compra
{
    public int CompraId { get; set; }
    public int ProveedorId { get; set; }
    public int EmpleadoId { get; set; }
    public string NumeroFacturaProveedor { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.EMITIDA;
    public string? Notas { get; set; }

    public string ProveedorNombre { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public decimal TotalCompra { get; set; }
    public List<DetalleCompra> Detalles { get; set; } = [];
}

public sealed class DetalleCompra
{
    public int DetalleCompraId { get; set; }
    public int CompraId { get; set; }
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
}

