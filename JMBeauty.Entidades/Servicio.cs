namespace JMBeauty.Entidades;

public sealed class CategoriaServicio
{
    public int CategoriaServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public string DescripcionCombo => Activo ? Nombre : $"{Nombre} (inactiva)";
}

public sealed class Servicio
{
    public int ServicioId { get; set; }
    public int CategoriaServicioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DuracionMinutos { get; set; }
    public decimal PrecioBase { get; set; }
    public bool Activo { get; set; } = true;

    // Propiedad de navegación
    public string CategoriaNombre { get; set; } = string.Empty;
    public string DescripcionCombo => Activo ? $"{Nombre} ({DuracionMinutos} min) - Lps. {PrecioBase:N2}" : $"{Nombre} (inactivo)";

    public override string ToString() => DescripcionCombo;
}

