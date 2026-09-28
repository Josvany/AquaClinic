using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaClinic.App.Pages
{
    public partial class EspecialidadOverview
    {
        [Inject]
        public IEspecialidadDataService EspecialidadDataService{ get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public ICollection<EspecialidadCitasViewModel> Especialidades { get; set; }

        protected async override Task OnInitializedAsync()
        {
            try
            {
                Especialidades = await EspecialidadDataService.GetAllEspecialidadesWithCitas(false);
            }
            catch (ApiException)
            {
                // Endpoint no disponible / error de la API: la pagina muestra la
                // lista vacia sin tumbar el render (nav incluido)
                Especialidades = new List<EspecialidadCitasViewModel>();
            }
        }

        protected async void OnIncluirHistorialChanged(ChangeEventArgs args)
        {
            try
            {
                if((bool)args.Value)
                {
                    Especialidades = await EspecialidadDataService.GetAllEspecialidadesWithCitas(true);
                }
                else
                {
                    Especialidades = await EspecialidadDataService.GetAllEspecialidadesWithCitas(false);
                }
            }
            catch (ApiException)
            {
                Especialidades = new List<EspecialidadCitasViewModel>();
            }
        }
    }
}
