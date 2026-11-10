namespace JMBeauty.Entidades;

public sealed class Cliente
{
    public int ClienteId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? HistorialAlergiasNotas { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    public string DescripcionCombo => Activo ? $"{NombreCompleto} ({Telefono})" : $"{NombreCompleto} (inactivo)";
}

