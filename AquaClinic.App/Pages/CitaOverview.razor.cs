using AquaClinic.App.Contracts;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AquaClinic.App.Pages
{
    public partial class CitaOverview
    {
        [Inject]
        public ICitaDataService CitaDataService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public ICollection<CitaListViewModel> Citas { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected async override Task OnInitializedAsync()
        {
            Citas = await CitaDataService.GetAllCitas();
        }

        protected void AddNewCita()
        {
            NavigationManager.NavigateTo("/citadetails");
        }

        [Inject]
        public HttpClient HttpClient { get; set; }

        protected async Task ExportCitas()
        {
            if (await JSRuntime.InvokeAsync<bool>("confirm", $"¿Desea exportar esta lista a Excel?"))
            {
                var response = await HttpClient.GetAsync("api/citas/export");
                response.EnsureSuccessStatusCode();
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"ReporteCitas{DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}.csv";
                await JSRuntime.InvokeAsync<object>("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
            }
        }
    }
}
