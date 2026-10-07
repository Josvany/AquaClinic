using AquaClinic.App.Contracts;
using AquaClinic.App.Localization;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// Modal de edicion de una cita desde el listado de la agenda interna:
    /// muestra el detalle y permite moverla a otro horario o cancelarla.
    /// Es la misma logica que la pagina /citadetails, sobre el dialogo.
    /// </summary>
    public partial class CitaDetalleDialog
    {
        [Inject]
        public ICitaDataService Citas { get; set; } = default!;

        [Inject]
        public DialogService Dialogs { get; set; } = default!;

        [Inject]
        public NotificadorUi Notificador { get; set; } = default!;

        /// <summary>Cita que se edita.</summary>
        [Parameter]
        public Guid CitaId { get; set; }

        /// <summary>Se invoca con true cuando la cita quedo movida o cancelada.</summary>
        [Parameter]
        public EventCallback<bool> CitaModificada { get; set; }

        private CitaDetalleViewModel? _cita;
        private bool _cargando = true;
        private string? _error;

        protected override async Task OnParametersSetAsync()
        {
            await RecargarAsync();
        }

        /// <summary>
        /// Mueve la cita a otro horario: el dialogo elige la nueva fecha y hora
        /// libre (la API valida disponibilidad) y, si la cita queda movida, este
        /// modal se cierra tambien y avisa al listado para que se actualice.
        /// </summary>
        private async Task MoverCita()
        {
            if (_cita is null)
            {
                return;
            }

            var movida = false;

            // OpenAsync completa cuando el dialogo de mover se cierra: despues
            // se puede cerrar este sin pisar al que esta encima.
            await Dialogs.OpenAsync<MoverCitaDialog>(TextoUi.T("Mover cita"),
                new Dictionary<string, object?>
                {
                    { "Cita", _cita },
                    { "CitaMovida", EventCallback.Factory.Create<bool>(this, m => movida = m) }
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

            if (movida)
            {
                await CitaModificada.InvokeAsync(true);
                Dialogs.Close();
            }
        }

        /// <summary>
        /// Cancela la cita tras confirmacion: el horario vuelve a
        /// quedar disponible para agendar.
        /// </summary>
        private async Task CancelarCita()
        {
            if (_cita is null)
            {
                return;
            }

            var confirmado = await Dialogs.Confirm(
                string.Format(
                    TextoUi.T("¿Cancelar la cita de {0} del {1}? El horario volverá a estar disponible para agendar."),
                    _cita.Cliente,
                    _cita.FechaInicioLocal.ToString("f", System.Globalization.CultureInfo.CurrentCulture)),
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

            var respuesta = await Citas.CancelarCitaAsync(_cita.CitaId);

            if (respuesta.Success)
            {
                Notificador.Exito(TextoUi.T("Cita cancelada. El horario quedó disponible."));
                await RecargarAsync();
                await CitaModificada.InvokeAsync(true);
            }
            else
            {
                Notificador.Error(respuesta.Message ?? TextoUi.T("No se pudo cancelar la cita."));
            }
        }

        private async Task RecargarAsync()
        {
            _cargando = true;
            _error = null;

            try
            {
                _cita = await Citas.ObtenerDetalleAsync(CitaId);

                if (_cita is null)
                {
                    _error = TextoUi.T("No se pudo cargar la cita. Verifica tu conexión.");
                }
            }
            catch (Exception)
            {
                _error = TextoUi.T("No se pudo cargar la cita. Verifica tu conexión.");
            }
            finally
            {
                _cargando = false;
            }
        }
    }
}
