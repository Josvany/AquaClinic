using AquaClinic.Application.Features.Citas;

namespace AquaClinic.Application.Contracts.Citas
{
    /// <summary>
    /// Notificaciones en tiempo real de cambios en la agenda (reservas suaves
    /// y citas nuevas), difundidas a todos los clientes conectados.
    /// La implementacion (SignalR) vive en la API.
    /// </summary>
    public interface IAgregadorAgenda
    {
        /// <summary>Notifica que un slot quedo en proceso de reserva (o se libero con null).</summary>
        Task NotificarReservaSuaveAsync(ReservaSuaveDto? reserva);

        /// <summary>Notifica que se confirmo una cita (los demas la veran como Ocupado).</summary>
        Task NotificarCitaCreadaAsync(ReservaSuaveDto cita);
    }
}
