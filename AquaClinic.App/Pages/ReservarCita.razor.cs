using AquaClinic.App.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    public partial class ReservarCita
    {
        [Inject]
        public DialogService DialogService { get; set; } = default!;

        private Radzen.Blazor.RadzenScheduler<OcupacionAgenda>? _scheduler;
        private List<OcupacionAgenda> _ocupaciones = [];
        private DateTime _fecha = DateTime.Today;
        private string? _errorCarga;

        // Seleccion en curso (reserva suave propia)
        private DateTime _seleccionInicio;
        private bool _tieneSeleccion;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await Agenda.ConectarAsync();
                Agenda.ReservaSuave += AlReservaSuave;
                Agenda.ReservaLiberada += AlReservaLiberada;
                Agenda.CitaCreada += AlCitaCreada;

                await CargarAgendaAsync();
            }
            catch (HttpRequestException)
            {
                _errorCarga = "No se pudo conectar con la API. Verifica que esté corriendo.";
            }
        }

        private async Task CargarAgendaAsync()
        {
            var inicio = AgendaCalendario.InicioDeSemana(_fecha);
            var ocupaciones = await Agenda.ObtenerAgendaAsync(inicio, inicio.AddDays(7));

            // Si un slot ya tiene cita confirmada, cualquier "Reservando" que se solape
            // es residuo (la reserva suave se libera aparte en la API): la hora se
            // muestra como ocupada, nunca como "en proceso de reserva".
            _ocupaciones = ocupaciones
                .Where(o => o.Estado != AgendaCalendario.Reservando ||
                            !ocupaciones.Any(c => c.Estado == AgendaCalendario.Ocupado &&
                                                  c.Inicio < o.Fin && c.Fin > o.Inicio))
                .ToList();

            StateHasChanged();
        }

        // ---------- Eventos en tiempo real ----------
        // Llegan en un hilo del thread pool (fuera del circuito): todo lo que toque
        // estado del componente debe ejecutarse via InvokeAsync (dispatcher del renderer).

        private void AlReservaSuave(OcupacionAgenda ocupacion)
        {
            _ = InvokeAsync(() =>
            {
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Inicio == ocupacion.Inicio))
                    .Append(ocupacion)
                    .ToList();
                _scheduler?.Reload();
                StateHasChanged();
            });
        }

        private void AlReservaLiberada(DateTime inicio)
        {
            _ = InvokeAsync(() =>
            {
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Inicio == inicio && o.Estado == AgendaCalendario.Reservando))
                    .ToList();
                _scheduler?.Reload();
                StateHasChanged();
            });
        }

        private void AlCitaCreada(OcupacionAgenda ocupacion)
        {
            _ = InvokeAsync(() =>
            {
                // La cita confirmada reemplaza CUALQUIER bloque que se solape
                // (incluido el "Reservando" de la seleccion en curso), no solo el
                // que coincide al instante: asi el slot pasa a verse como Ocupado.
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Inicio < ocupacion.Fin && o.Fin > ocupacion.Inicio))
                    .Append(ocupacion)
                    .ToList();
                _scheduler?.Reload();
                StateHasChanged();
            });
        }

        // ---------- Render de bloques: color segun estado ----------

        private void OnAppointmentRender(Radzen.SchedulerAppointmentRenderEventArgs<OcupacionAgenda> args)
        {
            // "Reservando": fondo dorado (el de Iniciar sesion), letras teal.
            // "Ocupado": fondo teal, letras en el dorado de Iniciar sesion.
            args.Attributes["style"] = args.Data?.Estado == AgendaCalendario.Reservando
                ? AgendaCalendario.EstiloReservando
                : AgendaCalendario.EstiloOcupado;
        }

        // ---------- Flujo de reserva ----------

        private async Task OnSlotSelectAsync(Radzen.SchedulerSlotSelectEventArgs args)
        {
            var inicio = args.Start;
            var fin = args.End;

            // No se puede reservar en el pasado (dia anterior o meses anteriores).
            if (inicio.Date < AgendaCalendario.HoyClinica)
            {
                Notificador.Advertencia("No puedes reservar en fechas pasadas. Elige hoy o un día posterior.");
                return;
            }

            // Ya ocupado (cita confirmada o reserva en curso): no abre modal
            if (_ocupaciones.Any(o => o.Inicio < fin && o.Fin > inicio))
            {
                Notificador.Advertencia("Ese horario no está disponible. Elige otro libre.");
                return;
            }

            // Reserva suave: los demas la veran como "Reservando"
            var ok = await Agenda.ReservarSuavementeAsync(inicio, fin);
            if (!ok)
            {
                Notificador.Advertencia("Alguien está reservando ese horario en este momento. Elige otro disponible.");
                await CargarAgendaAsync();
                return;
            }

            _seleccionInicio = inicio;
            _tieneSeleccion = true;

            _ocupaciones.Add(new OcupacionAgenda { Inicio = inicio, Fin = fin, Estado = AgendaCalendario.Reservando });
            _scheduler?.Reload();

            var resultado = await DialogService.OpenAsync<ReservaDialog>("Reservar cita",
                new Dictionary<string, object>
                {
                    { "FechaInicioLocal", args.Start },
                    { "FechaFinLocal", args.End },
                    { "ReservaConfirmada", EventCallback.Factory.Create(this, (Func<(bool, DateTime), Task>)(async t => await ReservaTerminadaAsync(t.Item1, t.Item2))) },
                    { "ReservaCancelada", EventCallback.Factory.Create(this, (Func<Task>)(async () => await ReservaCanceladaAsync())) }
                }!,
                new DialogOptions
                {
                    Width = "520px",
                    ShowClose = true,
                    CloseDialogOnEsc = true,
                    CloseDialogOnOverlayClick = true,
                    Draggable = false,
                    CssClass = "ag-dialog"
                });

            // El dialogo se cerro por la X, Esc o clic fuera: liberar la reserva suave
            await ReservaCanceladaAsync();
        }

        private async Task ReservaTerminadaAsync(bool exito, DateTime fechaLocal)
        {
            _tieneSeleccion = false;

            if (exito)
            {
                // La cita quedo creada: el bloque "Reservando" propio se quita de
                // inmediato y el slot pasa a verse como ocupado.
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Inicio == _seleccionInicio && o.Estado == AgendaCalendario.Reservando))
                    .ToList();
                _scheduler?.Reload();

                // Refrescar la agenda completa (trae la cita como "Ocupado")
                await CargarAgendaAsync();
            }
        }

        private async Task ReservaCanceladaAsync()
        {
            if (_tieneSeleccion)
            {
                _tieneSeleccion = false;
                await Agenda.LiberarReservaAsync(_seleccionInicio);
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Inicio == _seleccionInicio && o.Estado == AgendaCalendario.Reservando))
                    .ToList();
                _scheduler?.Reload();
            }
        }

        public async ValueTask DisposeAsync()
        {
            Agenda.ReservaSuave -= AlReservaSuave;
            Agenda.ReservaLiberada -= AlReservaLiberada;
            Agenda.CitaCreada -= AlCitaCreada;

            if (_tieneSeleccion)
            {
                await Agenda.LiberarReservaAsync(_seleccionInicio);
            }
        }
    }
}