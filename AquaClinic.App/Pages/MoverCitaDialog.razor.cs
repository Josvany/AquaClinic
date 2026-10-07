using AquaClinic.App.Contracts;
using AquaClinic.App.Localization;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// Mueve una cita a otro horario (detalle de cita, /citadetails).
    /// Reutiliza la UX de seleccion de NuevaCitaDialog: fecha + hora
    /// libre dentro del horario de atencion; la API valida bloqueos y
    /// solapamiento al mover.
    /// </summary>
    public partial class MoverCitaDialog : IAsyncDisposable
    {
        [Inject]
        public ICitaDataService Citas { get; set; } = default!;

        [Inject]
        public DialogService Dialogs { get; set; } = default!;

        [Inject]
        public NotificadorUi Notificador { get; set; } = default!;

        [Inject]
        public AgendaRealtimeService Agenda { get; set; } = default!;

        /// <summary>Cita que se mueve (cliente, hora actual y duracion).</summary>
        [Parameter]
        public CitaDetalleViewModel Cita { get; set; } = default!;

        /// <summary>Se invoca con true cuando la cita quedo movida.</summary>
        [Parameter]
        public EventCallback<bool> CitaMovida { get; set; }

        private List<OpcionHora> _horas = [];
        private List<OcupacionAgenda> _agendaSemana = [];

        private DateTime? _fecha;
        private OpcionHora? _hora;

        private bool _enviando;
        private string? _error;

        // Reserva suave propia mientras se completa el dialogo: los demas
        // usuarios (publico en /reservar y agenda interna) la ven como
        // bloque dorado.
        private bool _agendaConectada;
        private DateTime? _reservaInicio;

        /// <summary>Hora ofrecida por la clinica dentro del horario de atencion.</summary>
        public sealed class OpcionHora
        {
            public TimeSpan Hora { get; init; }
            public string Texto { get; set; } = string.Empty;

            /// <summary>Ya hay una cita confirmada que se solapa con esta hora.</summary>
            public bool Ocupada { get; set; }

            /// <summary>No es seleccionable (ya paso o esta ocupada).</summary>
            public bool NoSeleccionable => Ocupada || Pasada;

            /// <summary>La hora ya paso del dia actual (solo aplica para hoy).</summary>
            public bool Pasada { get; set; }
        }

        /// <summary>El rango [inicio, inicio+minutos) se solapa con una cita confirmada.</summary>
        private bool HorarioOcupado(DateTime inicio, int minutos) =>
            _agendaSemana.Any(o => o.Estado == AgendaCalendario.Ocupado &&
                                   o.Inicio < inicio.AddMinutes(minutos) && o.Fin > inicio);

        /// <summary>El nuevo horario es el mismo que el actual de la cita.</summary>
        private bool MismoHorarioActual(DateTime inicio)
        {
            var actual = Cita.FechaInicioLocal;
            return inicio.Date == actual.Date && inicio.TimeOfDay == actual.TimeOfDay;
        }

        protected override async Task OnInitializedAsync()
        {
            _fecha = Cita.FechaInicioLocal;
            _horas = ConstruirHoras();

            try
            {
                await Agenda.ConectarAsync();
                _agendaConectada = true;
            }
            catch (Exception)
            {
                // Sin agenda en vivo el dialogo funciona igual: la API
                // valida el horario al mover la cita.
            }

            await RecalcularDisponibilidadAsync();
        }

        // ---------- Seleccion de fecha/hora ----------

        private async Task AlCambiarFechaAsync(DateTime? fecha)
        {
            _fecha = fecha;

            if (fecha is not null && fecha.Value.Date < AgendaCalendario.HoyClinica)
            {
                _error = TextoUi.T("No puedes mover la cita a fechas pasadas. Elige hoy o un día posterior.");
                StateHasChanged();
                return;
            }

            _error = null;
            await RecalcularDisponibilidadAsync();
            RevincularHora();
            await ReReservarAsync();
        }

        private async Task AlCambiarHoraAsync(OpcionHora? hora)
        {
            _hora = hora;

            if (hora is { Ocupada: true })
            {
                _error = TextoUi.T("Ese horario ya está ocupado. Elige otro disponible.");
                return;
            }

            _error = null;
            await ReReservarAsync();
        }

        /// <summary>
        /// Al reconstruir la lista de horas, la seleccion apunta a una
        /// instancia vieja (el dropdown la borra con un Change(null));
        /// se vuelve a ligar a la hora equivalente de la lista nueva.
        /// </summary>
        private void RevincularHora()
        {
            if (_hora is not null)
            {
                _hora = _horas.FirstOrDefault(h => h.Hora == _hora.Hora);
            }
        }

        /// <summary>
        /// Trae la agenda de la semana de la fecha elegida y marca como
        /// ocupadas las horas que se solapan con una cita confirmada.
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
                    // Sin agenda no se pueden marcar; la API rechaza el
                    // solape al mover.
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

            var minutos = Cita.DuracionMinutos;
            var esHoy = _fecha.Value.Date == AgendaCalendario.HoyClinica;
            var ahora = AgendaCalendario.AhoraClinica;

            foreach (var hora in _horas)
            {
                hora.Ocupada = HorarioOcupado(_fecha.Value.Date.Add(hora.Hora), minutos);

                if (hora.Ocupada)
                {
                    hora.Texto += " (ocupada)";
                }

                // Hoy: las horas que ya pasaron no se pueden elegir.
                hora.Pasada = esHoy && _fecha.Value.Date.Add(hora.Hora) < ahora;
            }
        }

        /// <summary>
        /// Al cambiar fecha u hora libera la reserva anterior y marca la
        /// nueva para que otros usuarios la vean como "Reservando".
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
                var minutos = Cita.DuracionMinutos;

                if (inicio < AgendaCalendario.AhoraClinica || HorarioOcupado(inicio, minutos))
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

        // ---------- Acciones ----------

        private async Task Enviar()
        {
            _error = null;

            if (_fecha is null)
            {
                _error = TextoUi.T("Elige la nueva fecha de la cita.");
                return;
            }

            if (_fecha.Value.Date < AgendaCalendario.HoyClinica)
            {
                _error = TextoUi.T("No puedes mover la cita a fechas pasadas. Elige hoy o un día posterior.");
                return;
            }

            if (_hora is null)
            {
                _error = TextoUi.T("Elige la nueva hora de la cita.");
                return;
            }

            var inicioLocal = _fecha.Value.Date.Add(_hora.Hora);

            // Doble verificacion: la hora pudo quedar seleccionada y pasar
            // mientras se llenaba el dialogo.
            if (inicioLocal < AgendaCalendario.AhoraClinica)
            {
                _error = TextoUi.T("La hora elegida ya pasó. Elige una hora disponible.");
                return;
            }

            if (MismoHorarioActual(inicioLocal))
            {
                _error = TextoUi.T("Ese es el horario actual de la cita. Elige otro diferente.");
                return;
            }

            // Doble verificacion (la agenda pudo cambiar mientras se llenaba el dialogo).
            if (HorarioOcupado(inicioLocal, Cita.DuracionMinutos))
            {
                _error = TextoUi.T("Ese horario ya está ocupado. Elige otra hora.");
                return;
            }

            _enviando = true;

            try
            {
                var resultado = await Citas.MoverCitaAsync(
                    Cita.CitaId, inicioLocal, Cita.DuracionMinutos);

                if (resultado.Success)
                {
                    // El servidor ya libero la reserva suave de esta hora al
                    // confirmar el movimiento; el slot queda como Ocupado via
                    // el evento CitaCreada, asi que el dialogo deja de
                    // llevar la suya.
                    _reservaInicio = null;

                    Notificador.Exito(string.Format(
                        TextoUi.T("Cita movida para el {0} a las {1}."),
                        inicioLocal.ToString("D", System.Globalization.CultureInfo.CurrentCulture),
                        _hora.Texto));
                    await CitaMovida.InvokeAsync(true);
                    Dialogs.Close();
                    return;
                }

                _error = resultado.Message ?? TextoUi.T("No se pudo mover la cita.");
                Notificador.Error(TextoUi.T(_error));
            }
            catch (HttpRequestException)
            {
                _error = TextoUi.T("No se pudo conectar con la API. Verifica tu conexión.");
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
