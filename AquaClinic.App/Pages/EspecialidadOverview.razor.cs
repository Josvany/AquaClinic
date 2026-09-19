using AquaClinic.App.Contracts;
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
            Especialidades = await EspecialidadDataService.GetAllEspecialidadesWithCitas(false);
        }

        protected async void OnIncluirHistorialChanged(ChangeEventArgs args)
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
    }
}
