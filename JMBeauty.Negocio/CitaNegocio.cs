using JMBeauty.Datos;
using JMBeauty.Entidades;

namespace JMBeauty.Negocio;

public sealed class CitaNegocio
{
    private static void Validar(Cita cita)
    {
        ArgumentNullException.ThrowIfNull(cita);

        if (cita.ClienteId <= 0)
            throw new ArgumentException("Debe seleccionar un cliente para agendar la cita.");

        if (cita.EmpleadoId <= 0)
            throw new ArgumentException("Debe asignar un estilista / profesional a la cita.");

        if (cita.FechaHoraFin <= cita.FechaHoraInicio)
            throw new ArgumentException("La hora de finalización debe ser posterior a la hora de inicio.");

        if (cita.CitaId == 0 && cita.FechaHoraInicio < DateTime.Now.AddMinutes(-5))
            throw new ArgumentException("No se pueden agendar citas en fechas u horas pasadas.");

        if (cita.Servicios.Count == 0)
            throw new ArgumentException("Debe seleccionar al menos un servicio para la cita.");
    }

    public static async Task<List<Cita>> ListarPorRangoAsync(string clave, DateTime inicio, DateTime fin, int? empleadoId = null, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        using var conexion = new Conexion(clave);
        var dal = new CitaDatos(conexion);
        return await dal.ListarPorRangoAsync(inicio, fin, empleadoId, cancellationToken);
    }

    public static async Task<int> AgendarCitaAsync(string clave, Cita cita, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        Validar(cita);

        using var conexion = new Conexion(clave);
        var dal = new CitaDatos(conexion);

        var solapa = await dal.ExisteSolapamientoAsync(
            cita.EmpleadoId,
            cita.FechaHoraInicio,
            cita.FechaHoraFin,
            cita.CitaId > 0 ? cita.CitaId : null,
            cancellationToken);

        if (solapa)
        {
            throw new InvalidOperationException("El profesional seleccionado ya cuenta con otra cita agendada en ese rango de horario.");
        }

        return await dal.GuardarCitaCompletaAsync(cita, cancellationToken);
    }

    public static async Task CambiarEstadoAsync(string clave, int citaId, EstadoCita nuevoEstado, CancellationToken cancellationToken = default)
    {
        ConexionNegocio.ValidarClave(clave);
        if (citaId <= 0)
            throw new ArgumentException("Identificador de cita inválido.");

        using var conexion = new Conexion(clave);
        var dal = new CitaDatos(conexion);
        await dal.CambiarEstadoAsync(citaId, nuevoEstado, cancellationToken);
    }
}

