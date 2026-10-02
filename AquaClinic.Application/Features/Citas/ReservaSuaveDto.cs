namespace AquaClinic.Application.Features.Citas
{
    /// <summary>
    /// Reserva suave: un usuario selecciono un horario en el scheduler y esta
    /// completando sus datos (ventana de ~2 minutos). Mientras exista, los demas
    /// visitantes ven el slot como "Reservando" y no pueden tomarlo.
    /// </summary>
    public record ReservaSuaveDto(DateTimeOffset FechaInicio, DateTimeOffset FechaFin);

    /// <summary>Estado de ocupacion de un rango del scheduler publico.</summary>
    public enum EstadoSlot
    {
        /// <summary>Ya existe una cita confirmada/pendiente en ese rango.</summary>
        Ocupado = 1,

        /// <summary>Otro usuario esta completando la reserva (transitorio).</summary>
        Reservando = 2,

        /// <summary>Bloqueo de la clinica: dia/hora cerrada, no reservable.</summary>
        Bloqueado = 3
    }
}
