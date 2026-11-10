namespace JMBeauty.Entidades;

public sealed class CategoriaProducto
{
    public int CategoriaProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public string DescripcionCombo => Activo ? Nombre : $"{Nombre} (inactiva)";
}

public sealed class Producto
{
    public int ProductoId { get; set; }
    public int CategoriaProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal ExistenciaMinima { get; set; } = 5.00m;
    public bool EsInsumoInterno { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    // Propiedad de navegación
    public string CategoriaNombre { get; set; } = string.Empty;
    public string DescripcionCombo => Activo ? $"{Codigo} - {Nombre} (Lps. {PrecioVenta:N2})" : $"{Codigo} - {Nombre} (inactivo)";
}

