using AquaClinic.App.Contracts;
using AquaClinic.App.Localization;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    public partial class MisCitas
    {
        [Inject]
        public ICitaDataService CitaDataService { get; set; } = default!;

        [Inject]
        public DialogService Dialogs { get; set; } = default!;

        [Inject]
        public NotificadorUi Notificador { get; set; } = default!;

        public List<MiCitaViewModel> Citas { get; set; } = [];

        public bool Cargando { get; set; } = true;

        private string? _error;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Citas = await CitaDataService.GetMisCitas();
            }
            catch (Exception)
            {
                _error = "No se pudieron cargar tus citas. Intenta de nuevo m\u00e1s tarde.";
            }
            finally
            {
                Cargando = false;
            }
        }

        /// <summary>
        /// Abre el selector de nuevo horario para una cita propia que aun
        /// no ha pasado. Reutiliza MoverCiteDialog, que ya valida
        /// disponibilidad contra la agenda en vivo.
        /// </summary>
        private async Task MoverCita(MiCitaViewModel cita)
        {
            if (!cita.PermiteGestionar)
            {
                return;
            }

            var detalle = new CitaDetalleViewModel
            {
                CitaId = cita.CitaId,
                Cliente = TextoUi.T("Tu cita"),
                FechaInicioLocal = cita.Fecha,
                FechaFinLocal = cita.Fecha.AddMinutes(cita.DuracionMinutos),
                Estado = cita.Estado,
                EstadoValor = cita.EstadoValor,
                Notas = cita.Notas
            };

            await Dialogs.OpenAsync<MoverCitaDialog>(TextoUi.T("Cambiar cita"),
                new Dictionary<string, object>
                {
                    { "Cita", detalle },
                    { "EsCitaPropia", true },
                    { "CitaMovida", EventCallback.Factory.Create<bool>(this, async movida =>
                        {
                            if (movida)
                            {
                                await RecargarAsync();
                            }
                        }) }
                },
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

        /// <summary>
        /// Cancela una cita propia tras confirmacion: el horario vuelve a
        /// quedar disponible para agendar.
        /// </summary>
        private async Task CancelarCita(MiCitaViewModel cita)
        {
            if (!cita.PermiteGestionar)
            {
                return;
            }

            var confirmado = await Dialogs.Confirm(
                string.Format(
                    TextoUi.T("¿Cancelar la cita de {0} del {1}? El horario volverá a estar disponible para agendar."),
                    cita.Servicio,
                    cita.Fecha.ToString("f", System.Globalization.CultureInfo.CurrentCulture)),
                TextoUi.T("Cancelar cita"),
                new ConfirmOptions
                {
                    OkButtonText = TextoUi.T("Sí, cancelar"),
                    CancelButtonText = TextoUi.T("No, conservar"),
                    // Los botones toman el estilo del calendario (/reservar).
                    CssClass = "ag-confirm-calendario"
                }) == true;

            if (!confirmado)
            {
                return;
            }

            var respuesta = await CitaDataService.CancelarMisCitaAsync(cita.CitaId);

            if (respuesta.Success)
            {
                Notificador.Exito(TextoUi.T("Cita cancelada. El horario quedó disponible."));
                await RecargarAsync();
            }
            else
            {
                Notificador.Error(respuesta.Message ?? TextoUi.T("No se pudo cancelar la cita."));
            }
        }

        /// <summary>Vuelve a pedir las citas al servidor tras un movimiento.</summary>
        private async Task RecargarAsync()
        {
            try
            {
                Citas = await CitaDataService.GetMisCitas();
            }
            catch (Exception)
            {
                _error = "No se pudieron cargar tus citas. Intenta de nuevo m\u00e1s tarde.";
            }
        }
    }
}
