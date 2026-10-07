using AquaClinic.App.Localization;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// CRUD administrativo del catalogo de promociones. El markup vive en
    /// PromocionOverview.razor y la logica en este code-behind.
    /// </summary>
    public partial class PromocionOverview
    {
        private List<PromocionViewModel> _promociones = [];
        private bool _cargando = true;
        private string? _errorCarga;

        protected override async Task OnInitializedAsync()
        {
            await CargarAsync();
        }

        /// <summary>Vigencia legible: rango con ambos extremos, o solo el que este definido.</summary>
        private static string VigenciaTexto(PromocionViewModel p)
        {
            var cultura = System.Globalization.CultureInfo.CurrentCulture;

            if (p.VigenciaDesde is DateTime desde && p.VigenciaHasta is DateTime hasta)
            {
                return $"{desde.ToString("d", cultura)} — {hasta.ToString("d", cultura)}";
            }

            if (p.VigenciaDesde is DateTime soloDesde)
            {
                return string.Format(TextoUi.T("Desde {0}"), soloDesde.ToString("d", cultura));
            }

            return string.Format(TextoUi.T("Hasta {0}"),
                p.VigenciaHasta!.Value.ToString("d", cultura));
        }

        private async Task CargarAsync()
        {
            _cargando = true;

            try
            {
                _promociones = await PromocionDataService.ObtenerTodasAsync();
                _errorCarga = null;
            }
            catch (HttpRequestException)
            {
                _errorCarga = "No se pudo conectar con la API. Verifica que esté corriendo.";
            }
            finally
            {
                _cargando = false;
            }
        }

        private async Task AbrirCrearAsync()
        {
            await Dialogs.OpenAsync<PromocionFormDialog>(TextoUi.T("Nueva promoción"),
                new Dictionary<string, object> { { "OnGuardado", EventCallback.Factory.Create(this, CargarAsync) } },
                new DialogOptions { Width = "640px", ShowClose = true, CloseDialogOnEsc = true, CloseDialogOnOverlayClick = false, Draggable = false, CssClass = "ag-dialog" });
        }

        private async Task AbrirEditarAsync(PromocionViewModel promocion)
        {
            await Dialogs.OpenAsync<PromocionFormDialog>(TextoUi.T("Editar promoción"),
                new Dictionary<string, object>
                {
                    { "Promocion", promocion },
                    { "OnGuardado", EventCallback.Factory.Create(this, CargarAsync) }
                },
                new DialogOptions { Width = "640px", ShowClose = true, CloseDialogOnEsc = true, CloseDialogOnOverlayClick = false, Draggable = false, CssClass = "ag-dialog" });
        }

        private async Task ConfirmarEliminarAsync(PromocionViewModel promocion)
        {
            var confirmado = await Dialogs.Confirm(
                string.Format(TextoUi.T("¿Eliminar la promoción \"{0}\"?"), promocion.Nombre),
                TextoUi.T("Eliminar promoción"),
                new ConfirmOptions { OkButtonText = TextoUi.T("Eliminar"), CancelButtonText = TextoUi.T("Cancelar") }) == true;

            if (!confirmado)
            {
                return;
            }

            var respuesta = await PromocionDataService.EliminarAsync(promocion.PromocionId);

            if (respuesta.Success)
            {
                Notificador.Exito(string.Format(TextoUi.T("Promoción \"{0}\" eliminada."), promocion.Nombre));
                await CargarAsync();
            }
            else
            {
                var detalle = respuesta.Message
                    + (string.IsNullOrEmpty(respuesta.ValidationErrors) ? "" : Environment.NewLine + respuesta.ValidationErrors);
                Notificador.Error(TextoUi.T(detalle), TextoUi.T("No se pudo eliminar"));
            }
        }
    }
}