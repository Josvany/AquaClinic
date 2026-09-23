using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// Rango de tiempo ocupado/en proceso tal como lo ve el scheduler publico.
    /// RadzenScheduler trabaja con DateTime (hora local del servidor Blazor),
    /// asi que los rangos UTC de la API se convierten a local al recibirlos.
    /// </summary>
    public class OcupacionAgenda
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }

        /// <summary>"Ocupado" (cita confirmada) o "Reservando" (seleccion en curso).</summary>
        public string Estado { get; set; } = "Ocupado";
    }

    /// <summary>
    /// Conexion SignalR al AgendaHub de la API + consulta REST de la agenda semanal.
    /// Mantiene una sola conexion por circuito y expone los eventos en vivo:
    ///  - ReservaSuave / ReservaLiberada / ReservaRechazada (seleccion en curso)
    ///  - CitaCreada (se confirmo una cita)
    /// </summary>
    public class AgendaRealtimeService : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AgendaRealtimeService> _logger;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        private HubConnection? _conexion;

        public event Action<OcupacionAgenda>? ReservaSuave;
        public event Action<DateTime>? ReservaLiberada;
        public event Action<OcupacionAgenda>? CitaCreada;
        public event Action<Exception>? ConexionPerdida;

        public AgendaRealtimeService(HttpClient httpClient, IConfiguration configuration, ILogger<AgendaRealtimeService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>Conecta (si aun no hay conexion) y registra los handlers de eventos.</summary>
        public async Task ConectarAsync()
        {
            if (_conexion is { State: HubConnectionState.Connected })
            {
                return;
            }

            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7081/";
            _conexion = new HubConnectionBuilder()
                .WithUrl(new Uri(new Uri(baseUrl), "hubs/agenda"))
                .WithAutomaticReconnect()
                .Build();

            _conexion.On<DateTimeOffset, DateTimeOffset>("ReservaSuave", (inicio, fin) =>
                ReservaSuave?.Invoke(new OcupacionAgenda { Inicio = ALocal(inicio), Fin = ALocal(fin), Estado = "Reservando" }));

            _conexion.On<DateTimeOffset>("ReservaLiberada", inicio => ReservaLiberada?.Invoke(ALocal(inicio)));

            _conexion.On<DateTimeOffset, DateTimeOffset>("ReservaRechazada", (inicio, fin) =>
                ReservaSuave?.Invoke(new OcupacionAgenda { Inicio = ALocal(inicio), Fin = ALocal(fin), Estado = "Ocupado" }));

            _conexion.On<DateTimeOffset, DateTimeOffset>("CitaCreada", (inicio, fin) =>
                CitaCreada?.Invoke(new OcupacionAgenda { Inicio = ALocal(inicio), Fin = ALocal(fin), Estado = "Ocupado" }));

            _conexion.Closed += ex =>
            {
                if (ex is not null)
                {
                    _logger.LogWarning("Conexion a la agenda perdida: {Mensaje}", ex.Message);
                    ConexionPerdida?.Invoke(ex);
                }
                return Task.CompletedTask;
            };

            await _conexion.StartAsync();
        }

        /// <summary>Agenda semanal (bloques ocupados/reservando) via REST.</summary>
        public async Task<List<OcupacionAgenda>> ObtenerAgendaAsync(DateTime semanaInicio, DateTime semanaFin)
        {
            var url = $"api/citas/agenda?semanaInicio={semanaInicio:yyyy-MM-dd}&semanaFin={semanaFin:yyyy-MM-dd}";
            var agenda = await _httpClient.GetFromJsonAsync<AgendaApiDto>(url, _jsonOptions);

            return agenda?.Ocupaciones?.Select(o => new OcupacionAgenda
            {
                Inicio = ALocal(o.FechaInicio),
                Fin = ALocal(o.FechaFin),
                Estado = o.Estado
            }).ToList() ?? [];
        }

        /// <summary>Marca el rango como "en proceso de reserva" (los demas lo veran asi).</summary>
        public async Task<bool> ReservarSuavementeAsync(DateTime inicioLocal, DateTime finLocal)
        {
            if (_conexion is not { State: HubConnectionState.Connected })
            {
                return false;
            }

            await _conexion.InvokeAsync("ReservarSuavemente", AUtc(inicioLocal), AUtc(finLocal));
            return true;
        }

        /// <summary>Libera la seleccion en curso (modal cerrado o cambio de hora).</summary>
        public async Task LiberarReservaAsync(DateTime inicioLocal)
        {
            if (_conexion is { State: HubConnectionState.Connected })
            {
                await _conexion.SendAsync("LiberarReserva", AUtc(inicioLocal));
            }
        }

        // ---------- Conversiones de zona (la API habla UTC; el scheduler, DateTime local) ----------

        private static DateTime ALocal(DateTimeOffset utc)
        {
            return utc.LocalDateTime;
        }

        private static DateTimeOffset AUtc(DateTime local)
        {
            return new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local));
        }

        /// <summary>DTO crudo de la API (fechas UTC con offset).</summary>
        private sealed class OcupacionApiDto
        {
            public DateTimeOffset FechaInicio { get; set; }
            public DateTimeOffset FechaFin { get; set; }
            public string Estado { get; set; } = "Ocupado";
        }

        /// <summary>Respuesta cruda de GET api/citas/agenda.</summary>
        private sealed class AgendaApiDto
        {
            public List<OcupacionApiDto>? Ocupaciones { get; set; }
        }

        public async ValueTask DisposeAsync()
        {
            if (_conexion is not null)
            {
                await _conexion.DisposeAsync();
            }
        }
    }
}
