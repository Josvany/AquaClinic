using AquaClinic.App.Contracts;
using AquaClinic.App.Localization;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// CRUD administrativo del catalogo de especialidades. El markup vive en
    /// EspecialidadOverview.razor y la logica en este code-behind.
    /// </summary>
    public partial class EspecialidadOverview
    {
        private List<EspecialidadViewModel> _especialidades = [];
        private bool _cargando = true;
        private string? _errorCarga;

        protected override async Task OnInitializedAsync()
        {
            await CargarAsync();
        }

        private async Task CargarAsync()
        {
            _cargando = true;

            try
            {
                _especialidades = await EspecialidadDataService.ObtenerTodasAsync();
                _errorCarga = null;
            }
            catch (HttpRequestException)
            {
                _errorCarga = "No se pudo conectar con la API. Verifica que esté corriendo.";
            }
            catch (Exception)
            {
                // Cualquier otro fallo (respuesta inesperada, JSON invalido...)
                // deja la tabla vacia en vez de tumbar el render de la pagina.
                _especialidades = [];
                _errorCarga = "No se pudieron cargar las especialidades.";
            }
            finally
            {
                _cargando = false;
            }
        }

        private async Task AbrirCrearAsync()
        {
            var guardado = await Dialogs.OpenAsync<EspecialidadFormDialog>(
                TextoUi.T("Nueva especialidad"),
                new Dictionary<string, object>
                {
                    { "OnGuardado", EventCallback.Factory.Create(this, CargarAsync) }
                },
                DialogOpts());

            if (guardado is true)
            {
                Notificador.Exito(TextoUi.T("Especialidad creada."));
            }
        }

        private async Task AbrirEditarAsync(EspecialidadViewModel especialidad)
        {
            var guardado = await Dialogs.OpenAsync<EspecialidadFormDialog>(
                TextoUi.T("Editar especialidad"),
                new Dictionary<string, object>
                {
                    { "Especialidad", especialidad },
                    { "OnGuardado", EventCallback.Factory.Create(this, CargarAsync) }
                },
                DialogOpts());

            if (guardado is true)
            {
                Notificador.Exito(TextoUi.T("Especialidad actualizada."));
            }
        }

        private async Task ConfirmarEliminarAsync(EspecialidadViewModel especialidad)
        {
            var confirmado = await Dialogs.Confirm(
                string.Format(TextoUi.T("¿Eliminar la especialidad \"{0}\"?"), especialidad.Nombre),
                TextoUi.T("Eliminar especialidad"),
                new ConfirmOptions
                {
                    OkButtonText = TextoUi.T("Eliminar"),
                    CancelButtonText = TextoUi.T("Cancelar"),
                    CssClass = "ag-confirm-soft"
                }) == true;

            if (!confirmado)
            {
                return;
            }

            var respuesta = await EspecialidadDataService.EliminarAsync(especialidad.EspecialidadId);

            if (respuesta.Success)
            {
                Notificador.Exito(string.Format(
                    TextoUi.T("Especialidad \"{0}\" eliminada."), especialidad.Nombre));
                await CargarAsync();
            }
            else
            {
                var detalle = respuesta.Message
                    + (string.IsNullOrEmpty(respuesta.ValidationErrors)
                        ? ""
                        : Environment.NewLine + respuesta.ValidationErrors);

                Notificador.Error(TextoUi.T(detalle), TextoUi.T("No se pudo eliminar"));
            }
        }

        private static DialogOptions DialogOpts() => new()
        {
            Width = "560px",
            ShowClose = true,
            CloseDialogOnEsc = true,
            CloseDialogOnOverlayClick = false,
            Draggable = false,
            CssClass = "ag-dialog"
        };
    }
}