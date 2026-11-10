namespace JMBeauty.Entidades;

public sealed class InventarioItem
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal ExistenciaMinima { get; set; }
    public bool EsInsumoInterno { get; set; }
    public decimal ExistenciaActual { get; set; }
    public bool AlertaBajoMinimo { get; set; }
}

public sealed class ComisionResumen
{
    public int EmpleadoId { get; set; }
    public string Profesional { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal ComisionServicios { get; set; }
    public decimal ComisionProductos { get; set; }
    public decimal TotalComision { get; set; }
}

