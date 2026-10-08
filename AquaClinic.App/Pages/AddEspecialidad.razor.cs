using AquaClinic.App.Contracts;
using AquaClinic.App.Localization;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// Pagina de alta de especialidad (SSR + formulario simple). El CRUD completo
    /// vive en /especialidadoverview; esta se conserva por si se enlaza directo.
    /// </summary>
    public partial class AddEspecialidad
    {
        [Inject]
        public IEspecialidadDataService EspecialidadDataService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        public EspecialidadViewModel EspecialidadViewModel { get; set; } = new();

        public string? Message { get; set; }

        protected override void OnInitialized()
        {
            EspecialidadViewModel = new EspecialidadViewModel();
        }

        protected async Task HandleValidSubmit()
        {
            var response = await EspecialidadDataService.CrearAsync(EspecialidadViewModel);

            if (response.Success)
            {
                Message = TextoUi.T("Especialidad agregada");
            }
            else
            {
                Message = response.Message
                    + (string.IsNullOrEmpty(response.ValidationErrors)
                        ? ""
                        : Environment.NewLine + response.ValidationErrors);
            }
        }
    }
}