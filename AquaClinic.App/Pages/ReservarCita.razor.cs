using AquaClinic.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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

        // ------------------- Modo movil (vista dia) ----------------------
        // Un modulo JSInforma cambios de matchMedia("(max-width: 640px)").
        // El prerender estatico asume escritorio (vista semana) para no
        // divergir del HTML inicial; en el primer render interactivo el JS
        // corrige el valor si toca.
        private bool _esMovil;
        private DotNetObjectReference<ReservarCita>? _dotNetRef;
        private IJSObjectReference? _moduloMovil;
        private const string AnchoMovil = "(max-width: 640px)";

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private SeleccionReservaEstado SeleccionReserva { get; set; } = default!;

        // Seleccion en curso (reserva suave propia)
        private DateTime _seleccionInicio;
        private bool _tieneSeleccion;

        // Bandera sincrona anti-doble-apertura del modal (el evento SlotSelect
        // puede dispararse dos veces antes de que el primer await complete).
        private bool _abriendoModal;

        // True cuando el request llego con ?servicio=<id>: la URL se limpia en el
        // navegador apenas el circuito es interactivo (OnAfterRenderAsync), no antes.
        private bool _limpiarUrlDeSugerencia;

        protected override async Task OnInitializedAsync()
        {
            // Servicio sugerido desde fuera (ej. banner del index: /reservar?servicio=<id>).
            // Es de UN SOLO USO: se consume aqui; la URL se limpia despues, cuando la
            // pagina ya es interactiva (ver OnAfterRenderAsync), para que una recarga
            // no vuelva a preseleccionar el mismo servicio. Si el id no existe en el
            // catalogo el modal simplemente lo ignora.
            try
            {
                var query = new Uri(Navigation.Uri).Query;
                var params_ = System.Web.HttpUtility.ParseQueryString(query);
                var servicioRaw = params_["servicio"];

                if (Guid.TryParse(servicioRaw, out var servicioId) && servicioId != Guid.Empty)
                {
                    SeleccionReserva.EstablecerServicioSugerido(servicioId);
                    _limpiarUrlDeSugerencia = true;
                }
            }
            catch
            {
                // Query malformado: seguir sin preseleccion
            }

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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Quitar ?servicio= de la barra del navegador solo cuando el circuito ya
            // es interactivo: llamar NavigateTo durante el prerender estatico lanza
            // NavigationException. replace: true no deja entrada en el historial y una
            // recarga posterior ya no preselecciona el mismo servicio. Al ser navegacion dentro del
            // mismo circuito, el estado de SeleccionReserva (con la preseleccion del
            // modal) se conserva para esta visita.
            if (firstRender)
            {
                await IniciarDeteccionMovilAsync();

                if (_limpiarUrlDeSugerencia)
                {
                    _limpiarUrlDeSugerencia = false;

                    var uri = new Uri(Navigation.Uri);
                    if (uri.Query.Contains("servicio=", StringComparison.OrdinalIgnoreCase))
                    {
                        var limpia = uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped);
                        Navigation.NavigateTo(limpia, replace: true);
                    }
                }
            }
        }

        // ------------------- Deteccion movil -------------------

        private async Task IniciarDeteccionMovilAsync()
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                _moduloMovil = await JS.InvokeAsync<IJSObjectReference>("import", "./js/agenda-movil.js");
                _esMovil = await _moduloMovil.InvokeAsync<bool>("iniciar", AnchoMovil, _dotNetRef);
                if (_esMovil != _esMovilRenderizada)
                {
                    _esMovilRenderizada = _esMovil;
                    StateHasChanged();
                }
            }
            catch (JSException)
            {
                // Sin JS el fallback es escritorio: la vista semana sigue funcionando
            }
        }

        private bool _esMovilRenderizada;

        [JSInvokable]
        public void AlCambiarAncho(bool esMovil)
        {
            if (_esMovil == esMovil) return;
            _esMovil = esMovil;
            StateHasChanged();
        }

        // ------------------- Agenda -------------------

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

            // Domingo cerrado o dia vencido: el slot se ve deshabilitado y no
            // recibe clics (pointer-events: none), sin notificacion. El dia se
            // toma del INICIO del slot.
            if (args.Start.Date.DayOfWeek == DayOfWeek.Sunday || EsDiaVencido(args.Start))
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

        /// <summary>Dia cuya jornada (hasta las 19:00) ya termino (comparado con el
        /// momento actual en la zona de la clinica, America/Vancouver).
        /// Los dias anteriores los cubre el guard de fechas pasadas.</summary>
        private static bool EsDiaVencido(DateTime diaInicio) =>
            diaInicio.Date == AgendaCalendario.HoyClinica
                ? false // hoy nunca esta vencido: todavia puede quedar horario libre
                : diaInicio.Date < AgendaCalendario.HoyClinica;

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
                    { "FechaInicioLocal", inicio },
                    { "FechaFinLocal", fin },
                    { "Ocupaciones", _ocupaciones },
                    { "OcupacionesProvider", (Func<List<OcupacionAgenda>>)(() => _ocupaciones) },
                    { "ReservaConfirmada", EventCallback.Factory.Create(this, (Func<(bool, DateTime), Task>)(async t => await ReservaTerminadaAsync(t.Item1, t.Item2))) },
                    { "ReservaCancelada", EventCallback.Factory.Create(this, (Func<Task>)(async () => await ReservaCanceladaAsync())) },
                    { "ReservaActualizada", EventCallback.Factory.Create(this, (Func<(DateTime, int), Task>)(async ajuste => await ActualizarReservaAsync(ajuste))) }
                }!,
                new DialogOptions
                {
                    Width = "760px",
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

        /// <summary>Clic sobre un bloque "Ocupado"/"Reservando": sin accion (no
        /// abre el modal). Igual que los slots ocupados, que no son reservables.</summary>
        private void OnCitaSelectAsync(Radzen.SchedulerAppointmentSelectEventArgs<OcupacionAgenda> args)
        {
        }

        // ---------- Flujo de reserva (dia pasado) ----------

        /// <summary>Slot de HOY cuya hora ya paso (comparado con el momento actual
        /// en la zona de la clinica, America/Vancouver): no reservable, igual que
        /// los domingos. Los dias anteriores los cubre el guard de fechas pasadas.</summary>
        private static bool EsHoraPasada(DateTime slotInicio) =>
            slotInicio.Date == AgendaCalendario.HoyClinica && slotInicio <= AgendaCalendario.AhoraClinica;

        private async Task ReservaTerminadaAsync(bool exito, DateTime fechaLocal)
        {
            _tieneSeleccion = false;
            _abriendoModal = false;

            if (exito)
            {
                // La cita ya se creo: la proxima reserva abre el modal SIN servicio
                // preseleccionado (ni el sugerido del index ni el de la cita recien
                // agendada); el usuario elige de nuevo desde cero.
                SeleccionReserva.LimpiarPreseleccion();

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

            if (_dotNetRef is not null)
            {
                try
                {
                    if (_moduloMovil is not null)
                    {
                        await _moduloMovil.InvokeVoidAsync("detener");
                        await _moduloMovil.DisposeAsync();
                    }
                }
                catch (JSDisconnectedException)
                {
                    // El circuito se cerro antes que el modulo: nada que hacer
                }

                _dotNetRef.Dispose();
            }

            if (_tieneSeleccion)
            {
                await Agenda.LiberarReservaAsync(_seleccionInicio);
            }
        }
    }
}
