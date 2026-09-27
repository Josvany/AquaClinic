using AquaClinic.App.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    public partial class ReservarCita
    {
        [Inject]
        public DialogService DialogService { get; set; } = default!;

        [Inject]
        public TooltipService TooltipService { get; set; } = default!;

        private Radzen.Blazor.RadzenScheduler<OcupacionAgenda>? _scheduler;
        private List<OcupacionAgenda> _ocupaciones = [];
        private DateTime _fecha = DateTime.Today;
        private string? _errorCarga;

        // Seleccion en curso (reserva suave propia)
        private DateTime _seleccionInicio;
        private bool _tieneSeleccion;

        // Bandera sincrona anti-doble-apertura del modal (el evento SlotSelect
        // puede dispararse dos veces antes de que el primer await complete).
        private bool _abriendoModal;

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

        private void OnSlotRender(Radzen.SchedulerSlotRenderEventArgs args)
        {
            // Domingo cerrado: el slot se ve deshabilitado y no recibe clics
            // (pointer-events: none), sin necesidad de notificacion.
            if (args.Start.DayOfWeek == DayOfWeek.Sunday)
            {
                var claseBase = args.Attributes.TryGetValue("class", out var c) ? c?.ToString() : null;
                args.Attributes["class"] = $"{claseBase} ag-slot-deshabilitado".Trim();
            }
        }

        private void OnAppointmentRender(Radzen.SchedulerAppointmentRenderEventArgs<OcupacionAgenda> args)
        {
            // "Reservando": fondo dorado (el de Iniciar sesion), letras teal.
            // "Ocupado": fondo teal, letras en el dorado de Iniciar sesion.
            args.Attributes["style"] = args.Data?.Estado == AgendaCalendario.Reservando
                ? AgendaCalendario.EstiloReservando
                : AgendaCalendario.EstiloOcupado;
        }

        // ---------- Tooltip al pasar el mouse sobre una cita ----------

        private void AlMouseEnCita(Radzen.Blazor.SchedulerAppointmentMouseEventArgs<OcupacionAgenda> args)
        {
            if (args.Data is null) return;

            var cita = args.Data;
            var duracion = cita.Fin - cita.Inicio;

            // Duration = null: el tooltip permanece hasta que el mouse sale de la cita
            // (AlMouseFueraCita lo cierra); sin esto Radzen lo oculta a los 2 segundos.
            TooltipService.Open(args.Element, _ => ContenidoTooltip(cita, duracion),
                new TooltipOptions { Position = TooltipPosition.Right, Duration = null });
        }

        private RenderFragment ContenidoTooltip(OcupacionAgenda cita, TimeSpan duracion) => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "ag-tooltip-cita");
            builder.OpenElement(2, "strong");
            builder.AddContent(3, cita.Text);
            builder.CloseElement();
            builder.OpenElement(4, "span");
            builder.AddContent(5, $"{cita.Inicio:dd/MM/yyyy hh:mm tt} – {cita.Fin:hh:mm tt}");
            builder.CloseElement();
            builder.OpenElement(6, "span");
            builder.AddContent(7, $"Duración: {FormatoDuracion(duracion)}");
            builder.CloseElement();
            builder.CloseElement();
        };

        private void AlMouseFueraCita(Radzen.Blazor.SchedulerAppointmentMouseEventArgs<OcupacionAgenda> args)
        {
            TooltipService.Close();
        }

        /// <summary>Formatea la duracion en horas y minutos (ej. "1 h 30 min").</summary>
        private static string FormatoDuracion(TimeSpan d) =>
            (d.Hours, d.Minutes) switch
            {
                (0, var m) => $"{m} min",
                (var h, 0) => $"{h} h",
                (var h, var m) => $"{h} h {m} min"
            };

        // ---------- Flujo de reserva ----------

        private async Task OnSlotSelectAsync(Radzen.SchedulerSlotSelectEventArgs args)
        {
            // Evita abrir dos modales si el navegador dispara el evento dos veces
            // (dejaba un dialogo duplicado asomando detras, desalineado a la izquierda).
            if (_tieneSeleccion || _abriendoModal) return;
            _abriendoModal = true;

            var inicio = args.Start;
            var fin = args.End;

            // No se puede reservar en el pasado (dia anterior o meses anteriores).
            if (inicio.Date < AgendaCalendario.HoyClinica)
            {
                _abriendoModal = false;
                Notificador.Advertencia("No puedes reservar en fechas pasadas. Elige hoy o un día posterior.");
                return;
            }

            // Los domingos la clinica no atiende: los slots vienen deshabilitados
            // (OnSlotRender), no abren el modal.
            if (inicio.DayOfWeek == DayOfWeek.Sunday)
            {
                _abriendoModal = false;
                return;
            }

            // Ya ocupado (cita confirmada o reserva en curso): no abre modal
            if (_ocupaciones.Any(o => o.Inicio < fin && o.Fin > inicio))
            {
                _abriendoModal = false;
                Notificador.Advertencia("Ese horario no está disponible. Elige otro libre.");
                return;
            }

            // Reserva suave: los demas la veran como "Reservando"
            var ok = await Agenda.ReservarSuavementeAsync(inicio, fin);
            if (!ok)
            {
                _abriendoModal = false;
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
                    { "Ocupaciones", _ocupaciones },
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
            _abriendoModal = false;
        }

        private async Task ReservaTerminadaAsync(bool exito, DateTime fechaLocal)
        {
            _tieneSeleccion = false;
            _abriendoModal = false;

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