using AquaClinic.Application.Contracts.Citas;
using Microsoft.AspNetCore.SignalR;

namespace AquaClinic.Api.Realtime
{
    /// <summary>
    /// Hub de SignalR con el estado en vivo de la agenda de reservas.
    /// Cada visitante se conecta al cargar /reservar; los metodos permiten
    /// señalar una hora en proceso y liberarla.
    /// Eventos que recibe el cliente:
    ///  - "ReservaSuave": alguien esta completando una reserva en un rango.
    ///  - "ReservaLiberada": la reserva en proceso expiro o se libero.
    ///  - "CitaCreada": se confirmo una cita (el slot pasa a Ocupado).
    /// </summary>
    public class AgendaHub : Hub
    {
        private readonly IReservasSuavesStore _reservasSuaves;
        private readonly ILogger<AgendaHub> _logger;

        public AgendaHub(IReservasSuavesStore reservasSuaves, ILogger<AgendaHub> logger)
        {
            _reservasSuaves = reservasSuaves;
            _logger = logger;
        }

        /// <summary>Marca un horario como "en proceso de reserva" para los demas visitantes.</summary>
        public async Task ReservarSuavemente(DateTimeOffset inicio, DateTimeOffset fin)
        {
            var ok = _reservasSuaves.IntentarReservar(Context.ConnectionId, inicio, fin);

            if (!ok)
            {
                // Alguien mas llego primero: solo se informa al llamador
                await Clients.Caller.SendAsync("ReservaRechazada", inicio, fin);
                return;
            }

            await Clients.Others.SendAsync("ReservaSuave", inicio, fin);
        }

        /// <summary>Libera la reserva suave de esta conexion (modal cerrado / cambio de hora).</summary>
        public async Task LiberarReserva(DateTimeOffset inicio)
        {
            _reservasSuaves.LiberarPorInicio(inicio);

            await Clients.Others.SendAsync("ReservaLiberada", inicio);
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug("AgendaHub: conexion {ConexionId} unida al grupo agenda", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Si la conexion muere con un modal abierto, la hora se libera sola
            _reservasSuaves.LiberarPorConexion(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
