namespace JMBeauty.Entidades;

public sealed class Empleado
{
    public int EmpleadoId { get; set; }
    public int RolId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string ClaveAcceso { get; set; } = "1234";
    public decimal PorcentajeComisionServicio { get; set; }
    public decimal PorcentajeComisionProducto { get; set; }
    public string ColorAgenda { get; set; } = "#D4AF37";
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    // Propiedades de navegación / lectura en UI
    public string RolNombre { get; set; } = string.Empty;
    public string TipoAcceso { get; set; } = "ESPECIALISTA"; // ADMINISTRADOR, RECEPCIONISTA, ESPECIALISTA
    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    public string DescripcionCombo => Activo ? NombreCompleto : $"{NombreCompleto} (inactivo)";
}

