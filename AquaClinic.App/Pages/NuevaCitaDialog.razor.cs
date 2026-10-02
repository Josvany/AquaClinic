using AquaClinic.App.Contracts;
using AquaClinic.App.Localization;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// Agendado desde el back office (/citaoverview): la cita puede ser para un
    /// cliente registrado (se guarda su UsuarioId) o para un cliente no registrado
    /// (nombre, correo y telefono de invitado). Reutiliza POST api/citas, que valida
    /// horario de la clinica, bloqueos y solapamiento con otras citas.
    /// </summary>
    public partial class NuevaCitaDialog : IAsyncDisposable
    {
        [Inject]
        public IServiciosPublicosDataService Servicios { get; set; } = default!;

        [Inject]
        public IReservasDataService Reservas { get; set; } = default!;

        [Inject]
        public IUsuarioDataService Usuarios { get; set; } = default!;

        [Inject]
        public DialogService Dialogs { get; set; } = default!;

        [Inject]
        public NotificadorUi Notificador { get; set; } = default!;

        [Inject]
        public AgendaRealtimeService Agenda { get; set; } = default!;

        /// <summary>Se invoca con true cuando la cita quedo creada.</summary>
        [Parameter]
        public EventCallback<bool> CitaCreada { get; set; }

        private List<ServicioPublicoVm> _servicios = [];
        private List<ClienteBusquedaViewModel> _clientes = [];
        private List<OpcionHora> _horas = [];

        private bool _esRegistrado = true;
        private string _busquedaCliente = string.Empty;
        private string? _clienteId;
        private string _nombre = string.Empty;
        private string _correo = string.Empty;
        private string _telefono = string.Empty;
        private Guid _servicioId;
        private DateTime? _fecha;
        private OpcionHora? _hora;
        private string _notas = string.Empty;

        private bool _buscando;
        private bool _enviando;
        private string? _error;

        // Reserva suave propia mientras se completa el dialogo: los demas usuarios
        // (publico en /reservar y agenda interna) la ven como bloque dorado.
        private bool _agendaConectada;
        private DateTime? _reservaInicio;

        // Agenda de la semana en curso: para deshabilitar las horas que ya tienen
        // una cita confirmada y no permitir reservar sobre ellas.
        private List<OcupacionAgenda> _agendaSemana = [];

        /// <summary>Hora ofrecida por la clinica dentro del horario de atencion.</summary>
        public sealed class OpcionHora
        {
            public TimeSpan Hora { get; init; }
            public string Texto { get; set; } = string.Empty;

            /// <summary>Ya hay una cita confirmada que se solapa con esta hora.</summary>
            public bool Ocupada { get; set; }
        }

        /// <summary>Duracion de la cita segun el servicio elegido (60 min por defecto).</summary>
        private int DuracionSeleccionada =>
            _servicios.FirstOrDefault(s => s.ServicioId == _servicioId)?.DuracionMinutos ?? 60;

        /// <summary>El rango [inicio, inicio+minutos) se solapa con una cita confirmada.</summary>
        private bool HorarioOcupado(DateTime inicio, int minutos) =>
            _agendaSemana.Any(o => o.Estado == AgendaCalendario.Ocupado &&
                                   o.Inicio < inicio.AddMinutes(minutos) && o.Fin > inicio);

        /// <summary>La fecha es anterior al dia actual en la zona de la clinica.</summary>
        private static bool FechaPasada(DateTime fecha) =>
            fecha.Date < AgendaCalendario.HoyClinica;

        protected override async Task OnInitializedAsync()
        {
            _fecha = AgendaCalendario.HoyClinica.AddDays(1);
            _horas = ConstruirHoras();

            try
            {
                await Agenda.ConectarAsync();
                _agendaConectada = true;
            }
            catch (Exception)
            {
                // Sin agenda en vivo el dialogo funciona igual: la API valida el
                // horario al crear la cita.
            }

            try
            {
                _servicios = (await Servicios.ObtenerServiciosAsync()).ToList();
                _clientes = await Usuarios.BuscarClientesAsync(null);
            }
            catch (Exception)
            {
                _error = "No se pudo cargar el catálogo de servicios. Intenta de nuevo.";
                Notificador.Error(TextoUi.T(_error), TextoUi.T("No hay conexión"));
            }

            // Marcar las horas ya ocupadas de la fecha inicial del dialogo.
            await RecalcularDisponibilidadAsync();
        }

        // ---------- Reserva suave mientras se completa el dialogo ----------

        private async Task AlCambiarFechaAsync(DateTime? fecha)
        {
            _fecha = fecha;

            // No se agendan citas en dias (ni meses) anteriores al actual.
            if (fecha is not null && FechaPasada(fecha.Value))
            {
                _error = "No puedes agendar en fechas pasadas. Elige hoy o un día posterior.";
                StateHasChanged();
                return;
            }

            _error = null;
            await RecalcularDisponibilidadAsync();
            RevincularHora();
            await ReReservarAsync();
        }

        private async Task AlCambiarServicioAsync(Guid servicioId)
        {
            _servicioId = servicioId;
            _error = null;

            // La duracion del servicio cambia el rango que ocupa la cita.
            await RecalcularDisponibilidadAsync();
            RevincularHora();
            await ReReservarAsync();
        }

        /// <summary>
        /// Al reconstruir la lista de horas, la seleccion apunta a una instancia vieja
        /// (el dropdown la borra con un Change(null)); se vuelve a ligar a la hora
        /// equivalente de la lista nueva.
        /// </summary>
        private void RevincularHora()
        {
            if (_hora is not null)
            {
                _hora = _horas.FirstOrDefault(h => h.Hora == _hora.Hora);
            }
        }

        private async Task AlCambiarHoraAsync(OpcionHora? hora)
        {
            _hora = hora;

            // Horario con cita confirmada: se avisa de inmediato y no se reserva.
            if (hora is { Ocupada: true })
            {
                _error = "Ese horario ya está ocupado. Elige otro disponible.";
                return;
            }

            _error = null;
            await ReReservarAsync();
        }

        /// <summary>
        /// Trae la agenda de la semana de la fecha elegida y marca como ocupadas
        /// las horas que se solapan con una cita confirmada.
        /// </summary>
        private async Task RecalcularDisponibilidadAsync()
        {
            if (_fecha is not null && _agendaConectada)
            {
                try
                {
                    var inicio = AgendaCalendario.InicioDeSemana(_fecha.Value);
                    _agendaSemana = await Agenda.ObtenerAgendaAsync(inicio, inicio.AddDays(7));
                }
                catch (Exception)
                {
                    // Sin agenda no se pueden marcar; la API rechaza el solape al crear.
                }
            }

            MarcarHorasOcupadas();
        }

        private void MarcarHorasOcupadas()
        {
            _horas = ConstruirHoras();

            if (_fecha is null)
            {
                return;
            }

            var minutos = DuracionSeleccionada;

            foreach (var hora in _horas)
            {
                hora.Ocupada = HorarioOcupado(_fecha.Value.Date.Add(hora.Hora), minutos);

                if (hora.Ocupada)
                {
                    hora.Texto += " (ocupada)";
                }
            }
        }

        /// <summary>
        /// Al cambiar fecha u hora libera la reserva anterior y marca la nueva para
        /// que otros usuarios la vean como "Reservando".
        /// </summary>
        private async Task ReReservarAsync()
        {
            if (!_agendaConectada)
            {
                return;
            }

            if (_reservaInicio is not null)
            {
                await Agenda.LiberarReservaAsync(_reservaInicio.Value);
                _reservaInicio = null;
            }

            if (_fecha is not null && _hora is not null)
            {
                var inicio = _fecha.Value.Date.Add(_hora.Hora);
                var minutos = DuracionSeleccionada;

                // No reservar sobre un horario con cita confirmada.
                if (HorarioOcupado(inicio, minutos))
                {
                    return;
                }

                if (await Agenda.ReservarSuavementeAsync(inicio, inicio.AddMinutes(minutos)))
                {
                    _reservaInicio = inicio;
                }
            }
        }

        private async Task LiberarReservaPropiaAsync()
        {
            if (_reservaInicio is not null)
            {
                var inicio = _reservaInicio.Value;
                _reservaInicio = null;

                if (_agendaConectada)
                {
                    await Agenda.LiberarReservaAsync(inicio);
                }
            }
        }

        /// <summary>Citas cada 30 minutos dentro del horario de atencion (8:00 a 19:00).</summary>
        private static List<OpcionHora> ConstruirHoras()
        {
            var horas = new List<OpcionHora>();
            var referencia = DateTime.Today;

            for (var hora = new TimeSpan(8, 0, 0); hora < new TimeSpan(19, 0, 0); hora = hora.Add(TimeSpan.FromMinutes(30)))
            {
                horas.Add(new OpcionHora { Hora = hora, Texto = referencia.Add(hora).ToString("h:mm tt") });
            }

            return horas;
        }

        private async Task BuscarClientesAsync()
        {
            _buscando = true;
            try
            {
                _clientes = await Usuarios.BuscarClientesAsync(_busquedaCliente);

                // Si el cliente elegido ya no esta en los resultados, se limpia la seleccion
                if (_clienteId is not null && _clientes.All(c => c.UsuarioId != _clienteId))
                {
                    _clienteId = null;
                }
            }
            finally
            {
                _buscando = false;
            }
        }

        private void ElegirTipoCliente(bool registrado)
        {
            _esRegistrado = registrado;
            _error = null;
        }

        private async Task Enviar()
        {
            _error = null;

            if (_servicioId == Guid.Empty)
            {
                _error = "Elige el servicio de la cita.";
                return;
            }

            if (_fecha is null)
            {
                _error = "Elige la fecha de la cita.";
                return;
            }

            if (FechaPasada(_fecha.Value))
            {
                _error = "No puedes agendar en fechas pasadas. Elige hoy o un día posterior.";
                return;
            }

            if (_hora is null)
            {
                _error = "Elige la hora de la cita.";
                return;
            }

            var inicioLocal = _fecha.Value.Date.Add(_hora.Hora);

            // Doble verificacion (la agenda pudo cambiar mientras se llenaba el dialogo).
            if (HorarioOcupado(inicioLocal, DuracionSeleccionada))
            {
                _error = "Ese horario ya está ocupado. Elige otra hora.";
                return;
            }

            if (_esRegistrado && string.IsNullOrWhiteSpace(_clienteId))
            {
                _error = "Elige el cliente registrado (puedes buscar por nombre o correo).";
                return;
            }

            if (!_esRegistrado && string.IsNullOrWhiteSpace(_nombre))
            {
                _error = "Escribe el nombre del cliente.";
                return;
            }

            if (!_esRegistrado && string.IsNullOrWhiteSpace(_correo))
            {
                _error = "Escribe el correo del cliente.";
                return;
            }

            _enviando = true;

            try
            {
                var resultado = await Reservas.CrearCitaAsync(new ReservaCitaVm
                {
                    ServicioId = _servicioId,
                    FechaInicioLocal = inicioLocal,
                    DuracionMinutos = null,
                    UsuarioId = _esRegistrado ? _clienteId : null,
                    NombreInvitado = _esRegistrado ? null : _nombre,
                    EmailInvitado = _esRegistrado ? null : _correo,
                    TelefonoInvitado = _esRegistrado ? null : _telefono,
                    Notas = string.IsNullOrWhiteSpace(_notas) ? null : _notas
                });

                if (resultado.Success)
                {
                    // La cita confirmada libero la reserva suave en el servidor
                    // (evento CitaCreada -> ReservaLiberada): ya no hay que soltarla.
                    _reservaInicio = null;

                    Notificador.Exito(string.Format(
                        TextoUi.T("Cita agendada para el {0} a las {1}."),
                        inicioLocal.ToString("D", System.Globalization.CultureInfo.CurrentCulture),
                        _hora.Texto));
                    await CitaCreada.InvokeAsync(true);
                    Dialogs.Close();
                    return;
                }

                _error = resultado.Message ?? "No se pudo crear la cita.";
                Notificador.Error(TextoUi.T(_error));
            }
            catch (HttpRequestException)
            {
                _error = "No se pudo conectar con la API. Verifica tu conexión.";
                Notificador.Error(TextoUi.T(_error));
            }
            finally
            {
                _enviando = false;
            }
        }

        private async Task Cancelar()
        {
            await LiberarReservaPropiaAsync();
            Dialogs.Close();
        }

        public async ValueTask DisposeAsync()
        {
            // Cierre por la X o Esc: soltar la reserva suave pendiente.
            await LiberarReservaPropiaAsync();
        }
    }
}
