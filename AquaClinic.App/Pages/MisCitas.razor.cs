using AquaClinic.App.Contracts;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Pages
{
    public partial class MisCitas
    {
        [Inject]
        public ICitaDataService CitaDataService { get; set; } = default!;

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
    }
}
