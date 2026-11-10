namespace JMBeauty.Entidades;

public sealed class Cita
{
    public int CitaId { get; set; }
    public int ClienteId { get; set; }
    public int EmpleadoId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public EstadoCita Estado { get; set; } = EstadoCita.PROGRAMADA;
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }

    // Propiedades de presentación
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string ColorAgenda { get; set; } = "#D4AF37";
    public string ServiciosResumen { get; set; } = string.Empty;
    public decimal TotalEstimado { get; set; }

    public List<DetalleCitaServicio> Servicios { get; set; } = [];
}

public sealed class DetalleCitaServicio
{
    public int DetalleCitaId { get; set; }
    public int CitaId { get; set; }
    public int ServicioId { get; set; }
    public int DuracionMinutos { get; set; }
    public decimal PrecioEstimado { get; set; }

    public string ServicioCodigo { get; set; } = string.Empty;
    public string ServicioNombre { get; set; } = string.Empty;
}

