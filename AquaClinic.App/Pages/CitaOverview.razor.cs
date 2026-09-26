using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.Globalization;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// Agenda interna de citas: calendario del back office con las citas agendadas
    /// y, en vivo via SignalR, las reservas en proceso que los visitantes estan
    /// haciendo en la pagina publica /reservar (bloques dorados).
    /// El listado "Citas agendadas" es un RadzenDataGrid server-side (paginado en
    /// la API, ordenado por fecha descendente).
    /// </summary>
    public partial class CitaOverview : IAsyncDisposable
    {
        private const int TamanoPagina = 10;

        [Inject]
        public ICitaDataService CitaDataService { get; set; } = default!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        public HttpClient HttpClient { get; set; } = default!;

        [Inject]
        public AgendaRealtimeService Agenda { get; set; } = default!;

        [Inject]
        public DialogService DialogService { get; set; } = default!;

        private List<CitaListViewModel>? _citas;                   // pagina visible del grid (null = sin cargar)
        private List<CitaListViewModel> _citasCalendario = [];     // todas, para bloques del scheduler
        private int _totalCitas;
        private bool _cargandoPagina;

        private List<OcupacionAgenda> _eventos = [];
        private readonly List<OcupacionAgenda> _reservasEnProceso = [];
        private DateTime _fecha = DateTime.Today;
        private string? _errorCarga;
        private string? _errorExport;
        private Radzen.Blazor.RadzenScheduler<OcupacionAgenda>? _scheduler;
        private Radzen.Blazor.RadzenDataGrid<CitaListViewModel>? _grid;

        private bool HayReservasEnProceso => _reservasEnProceso.Count > 0;

        private string TextoReservasEnProceso => _reservasEnProceso.Count > 1
            ? $"Reservando ({_reservasEnProceso.Count})"
            : "Reservando";

        protected override async Task OnInitializedAsync()
        {
            await CargarCitasCalendarioAsync();
            await ConectarAgendaAsync();
        }

        /// <summary>Carga completa de citas para los bloques del calendario.</summary>
        private async Task CargarCitasCalendarioAsync()
        {
            try
            {
                _citasCalendario = await CitaDataService.GetAllCitas();
                ReconstruirEventos();
            }
            catch (ApiException)
            {
                _errorCarga = "No se pudieron cargar las citas. Verifica que tengas permiso para verlas y que la API esté corriendo.";
            }
            catch (HttpRequestException)
            {
                _errorCarga = "No se pudo conectar con la API. Verifica que esté corriendo.";
            }
        }

        /// <summary>
        /// Carga de una pagina del DataGrid (invocado por LoadData del grid; el
        /// pager de Radzen pasa args.Skip/args.Top). Orden fijo: fecha descendente,
        /// aplicado en la API.
        /// </summary>
        private async Task CargarPaginaAsync(LoadDataArgs args)
        {
            if (_cargandoPagina)
            {
                return;
            }

            var skip = args.Skip ?? 0;
            var top = (args.Top ?? 0) > 0 ? args.Top!.Value : TamanoPagina;
            var pagina = (int)Math.Floor((double)skip / top) + 1;

            try
            {
                _cargandoPagina = true;
                var respuesta = await CitaDataService.GetCitasPaginadas(pagina, top);
                _citas = respuesta.Items;
                _totalCitas = respuesta.Total;
                _errorCarga = null;
            }
            catch (ApiException)
            {
                _errorCarga = "No se pudieron cargar las citas. Verifica que tengas permiso para verlas y que la API esté corriendo.";
            }
            catch (HttpRequestException)
            {
                _errorCarga = "No se pudo conectar con la API. Verifica que esté corriendo.";
            }
            finally
            {
                _cargandoPagina = false;
            }

            // El grid server-side se alimenta de la Data asignada en el handler:
            // forzar re-render para que pinte la pagina recibida.
            StateHasChanged();
        }

        /// <summary>
        /// Recarga la pagina visible (usado cuando llega una cita nueva por SignalR).
        /// </summary>
        private async Task RefrescarPaginaVisibleAsync()
        {
            await _grid.Reload();
        }

        /// <summary>
        /// Conecta la agenda en vivo para ver las reservas en proceso (la seleccion de
        /// horario que alguien esta completando en /reservar) y las citas nuevas.
        /// </summary>
        private async Task ConectarAgendaAsync()
        {
            try
            {
                await Agenda.ConectarAsync();
                Agenda.ReservaSuave += AlReservaSuave;
                Agenda.ReservaLiberada += AlReservaLiberada;
                Agenda.CitaCreada += AlCitaCreada;

                await CargarReservasEnProcesoAsync();
            }
            catch (HttpRequestException)
            {
                // Sin agenda en vivo la lista de citas sigue funcionando; solo no se
                // ven las reservas en proceso ni las citas creadas por otros usuarios.
            }
        }

        private async Task CargarReservasEnProcesoAsync()
        {
            var inicio = AgendaCalendario.InicioDeSemana(_fecha);
            var agenda = await Agenda.ObtenerAgendaAsync(inicio, inicio.AddDays(7));

            _reservasEnProceso.Clear();
            _reservasEnProceso.AddRange(agenda.Where(o => o.Estado == AgendaCalendario.Reservando));
            ReconstruirEventos();
        }

        /// <summary>Bloques del calendario: citas paginadas visibles + reservas en proceso.</summary>
        private void ReconstruirEventos()
        {
            var bloques = new List<OcupacionAgenda>();

            foreach (var cita in _citasCalendario)
            {
                bloques.Add(new OcupacionAgenda
                {
                    Text = cita.Nombre,
                    Inicio = cita.Fecha,
                    Fin = cita.Fecha.AddHours(1),
                    Estado = AgendaCalendario.Ocupado
                });
            }

            foreach (var reserva in _reservasEnProceso)
            {
                bloques.Add(new OcupacionAgenda
                {
                    Text = AgendaCalendario.Reservando,
                    Inicio = reserva.Inicio,
                    Fin = reserva.Fin,
                    Estado = AgendaCalendario.Reservando
                });
            }

            _eventos = bloques.OrderBy(b => b.Inicio).ToList();
        }

        // ---------- Eventos en tiempo real ----------
        // Llegan en un hilo del thread pool (fuera del circuito): lo que toque estado
        // del componente se ejecuta via InvokeAsync (dispatcher del renderer).

        private void AlReservaSuave(OcupacionAgenda ocupacion)
        {
            _ = InvokeAsync(() =>
            {
                _reservasEnProceso.RemoveAll(r => r.Inicio == ocupacion.Inicio);
                _reservasEnProceso.Add(ocupacion);
                ReconstruirEventos();
                _scheduler?.Reload();
                StateHasChanged();
            });
        }

        private void AlReservaLiberada(DateTime inicio)
        {
            _ = InvokeAsync(() =>
            {
                _reservasEnProceso.RemoveAll(r => r.Inicio == inicio);
                ReconstruirEventos();
                _scheduler?.Reload();
                StateHasChanged();
            });
        }

        private void AlCitaCreada(OcupacionAgenda ocupacion)
        {
            _ = InvokeAsync(async () =>
            {
                // La reserva en proceso se confirmo: el bloque dorado desaparece y la
                // cita recien creada se refleja en la pagina visible del grid.
                _reservasEnProceso.RemoveAll(r => r.Inicio < ocupacion.Fin && r.Fin > ocupacion.Inicio);
                ReconstruirEventos();
                _scheduler?.Reload();

                await CargarCitasCalendarioAsync();
                await RefrescarPaginaVisibleAsync();
            });
        }

        // ---------- Render de bloques: color segun estado ----------

        private void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<OcupacionAgenda> args)
        {
            args.Attributes["style"] = args.Data?.Estado == AgendaCalendario.Reservando
                ? AgendaCalendario.EstiloReservando
                : AgendaCalendario.EstiloOcupado;
        }

        // ---------- Acciones ----------

        /// <summary>
        /// Agendar desde el back office: el dialogo permite elegir un cliente
        /// registrado o capturar los datos de un cliente no registrado.
        /// </summary>
        protected async Task AddNewCita()
        {
            await DialogService.OpenAsync<NuevaCitaDialog>("Nueva cita",
                new Dictionary<string, object>
                {
                    { "CitaCreada", EventCallback.Factory.Create<bool>(this, async creada => await AlCrearCitaAsync(creada)) }
                }!,
                new DialogOptions
                {
                    Width = "560px",
                    ShowClose = true,
                    CloseDialogOnEsc = true,
                    CloseDialogOnOverlayClick = false,
                    Draggable = false,
                    CssClass = "ag-dialog"
                });
        }

        private async Task AlCrearCitaAsync(bool creada)
        {
            if (creada)
            {
                await CargarCitasCalendarioAsync();
                await RefrescarPaginaVisibleAsync();
            }
        }

        protected async Task ExportCitas()
        {
            if (await JSRuntime.InvokeAsync<bool>("confirm", "¿Desea exportar esta lista a Excel?"))
            {
                try
                {
                    var response = await HttpClient.GetAsync("api/citas/export");
                    response.EnsureSuccessStatusCode();
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = $"ReporteCitas{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.csv";
                    await JSRuntime.InvokeAsync<object>("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
                }
                catch (HttpRequestException)
                {
                    _errorExport = "No se pudo exportar la lista de citas. Inténtalo de nuevo más tarde.";
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            Agenda.ReservaSuave -= AlReservaSuave;
            Agenda.ReservaLiberada -= AlReservaLiberada;
            Agenda.CitaCreada -= AlCitaCreada;

            return ValueTask.CompletedTask;
        }
    }
}
