using AquaClinic.App.Contracts;
using AquaClinic.App.Localization;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    public partial class CitaDetails
    {
        [Inject]
        public ICitaDataService Citas { get; set; } = default!;

        [Inject]
        public DialogService Dialogs { get; set; } = default!;

        [Inject]
        public NotificadorUi Notificador { get; set; } = default!;

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Parameter]
        public string? CitaId { get; set; }

        private Guid SelectedCitaId = Guid.Empty;
        private CitaDetalleViewModel? Cita { get; set; }
        private bool Cargando { get; set; } = true;
        private string? Error { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!Guid.TryParse(CitaId, out SelectedCitaId) || SelectedCitaId == Guid.Empty)
            {
                Error = TextoUi.T("No se indicó la cita a consultar.");
                Cargando = false;
                return;
            }

            await RecargarAsync();
        }

        /// <summary>
        /// Mueve la cita a otro horario: el dialogo elige la nueva
        /// fecha y hora libre (la API valida disponibilidad).
        /// </summary>
        private async Task MoverCita()
        {
            if (Cita is null)
            {
                return;
            }

            await Dialogs.OpenAsync<MoverCitaDialog>(TextoUi.T("Mover cita"),
                new Dictionary<string, object>
                {
                    { "Cita", Cita },
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
        /// Cancela la cita tras confirmacion: el horario vuelve a
        /// quedar disponible para agendar.
        /// </summary>
        private async Task CancelarCita()
        {
            if (Cita is null)
            {
                return;
            }

            var confirmado = await Dialogs.Confirm(
                string.Format(
                    TextoUi.T("¿Cancelar la cita de {0} del {1}? El horario volverá a estar disponible para agendar."),
                    Cita.Cliente,
                    Cita.FechaInicioLocal.ToString("f", System.Globalization.CultureInfo.CurrentCulture)),
                TextoUi.T("Cancelar cita"),
                new ConfirmOptions
                {
                    OkButtonText = TextoUi.T("Cancelar cita"),
                    CancelButtonText = TextoUi.T("No, conservar")
                }) == true;

            if (!confirmado)
            {
                return;
            }

            var respuesta = await Citas.CancelarCitaAsync(Cita.CitaId);

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

        private async Task RecargarAsync()
        {
            Cargando = true;
            Error = null;

            try
            {
                Cita = await Citas.ObtenerDetalleAsync(SelectedCitaId);

                if (Cita is null)
                {
                    Error = TextoUi.T("No se pudo cargar la cita. Verifica tu conexión.");
                }
            }
            catch (Exception)
            {
                Error = TextoUi.T("No se pudo cargar la cita. Verifica tu conexión.");
            }
            finally
            {
                Cargando = false;
            }
        }

        private void VolverAlListado()
        {
            Navigation.NavigateTo("/citaoverview");
        }
    }
}
