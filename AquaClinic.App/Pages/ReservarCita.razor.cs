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
            // Radzen dibuja la semana segun su cultura (en esta instalacion
            // domingo->sabado) y la API filtra por rango UTC. Se consulta con un
            // dia de margen a cada lado para cubrir la semana visible sin depender
            // de la convencion (domingo o lunes) del primer dia; Radzen ignora
            // los bloques que caen fuera de la vista.
            var inicio = _fecha.Date.AddDays(-((int)_fecha.DayOfWeek) - 1);
            var ocupaciones = await Agenda.ObtenerAgendaAsync(inicio, inicio.AddDays(9));

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

        // ---------- Navegacion de semanas ----------
        // Las flechas/Hoy de Radzen cambian la fecha interna del scheduler; sin
        // este handler la pagina nunca pedia la agenda de la nueva semana y las
        // citas futuras no aparecian al navegar.
        private async Task OnFechaCambioAsync(DateTime fecha)
        {
            if (fecha.Date == _fecha.Date) return;
            _fecha = fecha;
            await CargarAgendaAsync();
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
            var claseBase = args.Attributes.TryGetValue("class", out var c) ? c?.ToString() : null;

            // Domingo cerrado o hora de hoy ya pasada: el slot se ve deshabilitado
            // y no recibe clics (pointer-events: none), sin notificacion.
            if (args.Start.DayOfWeek == DayOfWeek.Sunday || EsHoraPasada(args.Start))
            {
                args.Attributes["class"] = $"{claseBase} ag-slot-deshabilitado".Trim();
                return;
            }

            var ocupado = _ocupaciones.Any(o => o.Inicio < args.End && o.Fin > args.Start);

            // Ocupado (cita o "Reservando"): fondo teal translucido + tooltip.
            // Libre: tooltip al pasar el mouse (estilo .ag-slot-libre, CSS puro).
            if (ocupado)
            {
                args.Attributes["data-ag-tooltip"] = $"{args.Start:hh:mm tt} – {args.End:hh:mm tt}";
                args.Attributes["class"] = $"{claseBase} ag-slot-ocupado".Trim();
            }
            else
            {
                args.Attributes["data-ag-tooltip"] = $"{args.Start:hh:mm tt} – {args.End:hh:mm tt}";
                args.Attributes["class"] = $"{claseBase} ag-slot-libre".Trim();
            }
        }

        /// <summary>Slot de HOY cuya hora ya paso (comparado con el momento actual
        /// en la zona de la clinica, America/Vancouver): no reservable, igual que
        /// los domingos. Los dias anteriores los cubre el guard de fechas pasadas.</summary>
        private static bool EsHoraPasada(DateTime slotInicio) =>
            slotInicio.Date == AgendaCalendario.HoyClinica && slotInicio <= AgendaCalendario.AhoraClinica;

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

            // Refrescar la agenda ANTES de decidir: la lista puede estar vieja si
            // otra persona agendo mientras la pagina estaba abierta y la notificacion
            // en tiempo real no llego (ej. la cita se creo desde otro canal).
            try
            {
                await CargarAgendaAsync();
            }
            catch (HttpRequestException)
            {
                // Sin conexion se continua con los datos en memoria; la API
                // rechazara de todos modos si el horario esta tomado.
            }

            // No se puede reservar en el pasado (dia anterior o meses anteriores).
            if (inicio.Date < AgendaCalendario.HoyClinica)
            {
                _abriendoModal = false;
                Notificador.Advertencia("No puedes reservar en fechas pasadas. Elige hoy o un día posterior.");
                return;
            }

            // Hora de hoy ya pasada: los slots vienen grises sin clic (OnSlotRender);
            // este guard cubre el caso de una pagina abierta mientras corria el reloj
            // (el render de slots no se re-ejecuta solo). Silencioso: el usuario ve
            // el slot gris, un toast de mas seria ruido.
            if (EsHoraPasada(inicio))
            {
                _abriendoModal = false;
                return;
            }

            // Los domingos la clinica no atiende: los slots vienen deshabilitados
            // (OnSlotRender), no abren el modal.
            if (inicio.DayOfWeek == DayOfWeek.Sunday)
            {
                _abriendoModal = false;
                return;
            }

            // Slot ocupado (o que toca una ocupacion: el clic sobre el borde del
            // bloque "Ocupado" redondea al slot contiguo, que tambien esta ocupado).
            // Solo aviso: NO se abre modal ni se toma reserva suave. Las sugerencias
            // de horas alternas se ven al reservar en un slot libre y elegir un
            // servicio que no cabe completa (ReservaDialog).
            var tocaOcupacion = _ocupaciones.Any(o => o.Inicio < fin && o.Fin > inicio);
            if (tocaOcupacion)
            {
                _abriendoModal = false;
                Notificador.Advertencia("Ese horario no está disponible. Elige un slot libre.");
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

            // El bloque "Reservando" propio arranca con el slot de 15 min del clic;
            // el modal lo re-ancla a otra hora (sugerencias) o lo extiende a la
            // duracion del servicio elegido, y la pagina lo sincroniza via
            // ActualizarReservaAsync.
            _ocupaciones.Add(new OcupacionAgenda { Inicio = inicio, Fin = fin, Estado = AgendaCalendario.Reservando });
            _scheduler?.Reload();

            var resultado = await DialogService.OpenAsync<ReservaDialog>("Reservar cita",
                new Dictionary<string, object>
                {
                    { "FechaInicioLocal", args.Start },
                    { "FechaFinLocal", args.End },
                    { "Ocupaciones", _ocupaciones },
                    { "OcupacionesProvider", (Func<List<OcupacionAgenda>>)(() => _ocupaciones) },
                    { "ReservaConfirmada", EventCallback.Factory.Create(this, (Func<(bool, DateTime), Task>)(async t => await ReservaTerminadaAsync(t.Item1, t.Item2))) },
                    { "ReservaCancelada", EventCallback.Factory.Create(this, (Func<Task>)(async () => await ReservaCanceladaAsync())) },
                    { "ReservaActualizada", EventCallback.Factory.Create(this, (Func<(DateTime, int), Task>)(async ajuste => await ActualizarReservaAsync(ajuste))) }
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
                // La cita pudo haberse re-anclado a otra hora (sugerencias del
                // modal): se quita el bloque "Reservando" que coincide con la
                // hora FINAL usada por el modal, no necesariamente el inicial.
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Inicio == fechaLocal && o.Estado == AgendaCalendario.Reservando))
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

        /// <summary>
        /// El modal cambio el alcance de la reserva suave: elegir servicio (la
        /// sesion completa manda) o re-anclar a una hora sugerida. El bloque
        /// "Reservando" abarca inicio..inicio+duracion para todos los visitantes.
        /// </summary>
        private async Task ActualizarReservaAsync((DateTime Inicio, int DuracionMinutos) ajuste)
        {
            if (!_tieneSeleccion) return;

            var duracion = ajuste.DuracionMinutos > 0 ? ajuste.DuracionMinutos : 15;
            var inicio = ajuste.Inicio;
            var fin = inicio.AddMinutes(duracion);

            // Mismo inicio: la reserva suave en la API se identifica por la
            // conexion, asi que re-tomar el rango la reemplaza (los demas ven el
            // ReservaSuave actualizado con el fin nuevo). Cambio de hora: hay que
            // liberar la vieja primero (LiberarPorInicio) y despues tomar la nueva.
            if (inicio != _seleccionInicio)
            {
                await Agenda.LiberarReservaAsync(_seleccionInicio);
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Inicio == _seleccionInicio && o.Estado == AgendaCalendario.Reservando))
                    .ToList();
                _seleccionInicio = inicio;
            }

            var ok = await Agenda.ReservarSuavementeAsync(inicio, fin);
            if (ok)
            {
                _ocupaciones = _ocupaciones
                    .Where(o => !(o.Estado == AgendaCalendario.Reservando &&
                                  o.Inicio < fin && o.Fin > inicio))
                    .Append(new OcupacionAgenda { Inicio = inicio, Fin = fin, Estado = AgendaCalendario.Reservando })
                    .ToList();
                _scheduler?.Reload();
                StateHasChanged();
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