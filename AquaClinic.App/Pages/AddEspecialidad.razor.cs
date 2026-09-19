using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace AquaClinic.App.Pages
{
    public partial class AddEspecialidad
    {
        [Inject]
        public IEspecialidadDataService EspecialidadDataService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public EspecialidadViewModel EspecialidadViewModel { get; set; }
        public string Message { get; set; }

        protected override void OnInitialized()
        {
            EspecialidadViewModel = new EspecialidadViewModel();
        }

        protected async Task HandleValidSubmit()
        {
            var response = await EspecialidadDataService.CreateEspecialidad(EspecialidadViewModel);
            HandleResponse(response);
        }

        private void HandleResponse(ApiResponse<EspecialidadDto> response)
        {
            if (response.Success)
            {
                Message = "Especialidad agregada";
            }
            else
            {
                Message = response.Message;
                if (!string.IsNullOrEmpty(response.ValidationErrors))
                    Message += response.ValidationErrors;
            }
        }
    }
}
