using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Features.Citas;
using Microsoft.AspNetCore.SignalR;

namespace AquaClinic.Api.Realtime
{
    /// <summary>
    /// Difunde los cambios de agenda (reservas suaves y citas confirmadas) a
    /// todos los clientes conectados al AgendaHub.
    /// </summary>
    public class AgendaNotificador : IAgregadorAgenda
    {
        private readonly IHubContext<AgendaHub> _hub;
        private readonly IReservasSuavesStore _reservasSuaves;

        public AgendaNotificador(IHubContext<AgendaHub> hub, IReservasSuavesStore reservasSuaves)
        {
            _hub = hub;
            _reservasSuaves = reservasSuaves;
        }

        public async Task NotificarReservaSuaveAsync(ReservaSuaveDto? reserva)
        {
            if (reserva is null)
            {
                return;
            }

            await _hub.Clients.All.SendAsync("ReservaSuave", reserva.FechaInicio, reserva.FechaFin);
        }

        public async Task NotificarCitaCreadaAsync(ReservaSuaveDto cita)
        {
            // La cita confirmada sustituye a la reserva suave en los calendarios abiertos
            await _hub.Clients.All.SendAsync("CitaCreada", cita.FechaInicio, cita.FechaFin);
        }
    }
}