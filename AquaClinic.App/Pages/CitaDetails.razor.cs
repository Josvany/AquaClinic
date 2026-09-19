using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AquaClinic.App.Pages
{
    public partial class CitaDetails
    {
        [Inject]
        public ICitaDataService CitaDataService { get; set; }

        [Inject]
        public IEspecialidadDataService EspecialidadDataService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public CitaDetailViewModel CitaDetailViewModel { get; set; } 
            = new CitaDetailViewModel() { Fecha = DateTime.Now.AddDays(1) };

        public ObservableCollection<EspecialidadViewModel> Especialidades { get; set; } 
            = new ObservableCollection<EspecialidadViewModel>();

        public string Message { get; set; }
        public string SelectedEspecialidadId { get; set; }

        [Parameter]
        public string CitaId { get; set; }
        private Guid SelectedCitaId = Guid.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (Guid.TryParse(CitaId, out SelectedCitaId))
            {
                CitaDetailViewModel = await CitaDataService.GetCitaById(SelectedCitaId);
                SelectedEspecialidadId = CitaDetailViewModel.EspecialidadId.ToString();
            }

            var list = await EspecialidadDataService.GetAllEspecialidades();
            Especialidades = new ObservableCollection<EspecialidadViewModel>(list);
            SelectedEspecialidadId = Especialidades.FirstOrDefault().EspecialidadId.ToString();
        }

        protected async Task HandleValidSubmit()
        {
            CitaDetailViewModel.EspecialidadId = Guid.Parse(SelectedEspecialidadId);
            ApiResponse<Guid> response;

            if (SelectedCitaId == Guid.Empty)
            {
                response = await CitaDataService.CreateCita(CitaDetailViewModel);
            }
            else
            {
                 response = await CitaDataService.UpdateCita(CitaDetailViewModel);
            }
            HandleResponse(response);

        }

        protected async Task DeleteCita()
        {
            var response = await CitaDataService.DeleteCita(SelectedCitaId);
            HandleResponse(response);
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/citaoverview");
        }

        private void HandleResponse(ApiResponse<Guid> response)
        {
            if (response.Success)
            {
                NavigationManager.NavigateTo("/citaoverview");
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
