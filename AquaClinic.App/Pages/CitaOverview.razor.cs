using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private DateTime _fecha = DateTime.Today;
        private IList<AppointmentData> _eventos = new List<AppointmentData>();
        private string? _errorCarga;
        private string? _errorExport;

        protected async override Task OnInitializedAsync()
        {
            try
            {
                Citas = await CitaDataService.GetAllCitas();
                _eventos = Citas?.Select(ACitaAEvento).ToList() ?? new List<AppointmentData>();
            }
            catch (ApiException)
            {
                _errorCarga = "No se pudieron cargar las citas. Verifica que tengas permiso para verlas y que la API esté corriendo.";
            }
            catch (HttpRequestException)
            {
                _errorCarga = "No se pudo conectar con la API. Verifica que esté corriendo.";
            }
        }

        private static AppointmentData ACitaAEvento(CitaListViewModel cita) => new()
        {
            Text = cita.Nombre,
            Start = cita.Fecha,
            End = cita.Fecha.AddHours(1)
        };

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
                try
                {
                    var response = await HttpClient.GetAsync("api/citas/export");
                    response.EnsureSuccessStatusCode();
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = $"ReporteCitas{DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}.csv";
                    await JSRuntime.InvokeAsync<object>("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
                }
                catch (HttpRequestException)
                {
                    _errorExport = "No se pudo exportar la lista de citas. Inténtalo de nuevo más tarde.";
                }
            }
        }
    }
}
