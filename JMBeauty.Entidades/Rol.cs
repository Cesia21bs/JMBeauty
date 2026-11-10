namespace JMBeauty.Entidades;

public sealed class Rol
{
    public int RolId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string TipoAcceso { get; set; } = "ESPECIALISTA"; // ADMINISTRADOR, RECEPCIONISTA, ESPECIALISTA
    public bool Activo { get; set; } = true;

    public string DescripcionCombo => Activo ? Nombre : $"{Nombre} (inactivo)";
}

