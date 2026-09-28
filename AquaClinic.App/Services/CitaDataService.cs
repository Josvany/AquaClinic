using AutoMapper;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using System.Net.Http.Json;

namespace AquaClinic.App.Services
{
    public class CitaDataService: BaseDataService, ICitaDataService
    {
        
        private readonly IMapper _mapper;

        public CitaDataService(IClient client, IMapper mapper) : base(client)
        {
            _mapper = mapper;
        }

        public async Task<List<CitaListViewModel>> GetAllCitas()
        {
            var allCitas = await _client.GetAllCitasAsync();
            var mappedCitas = _mapper.Map<ICollection<CitaListViewModel>>(allCitas);
            return mappedCitas.ToList();
        }

        /// <summary>
        /// Pagina del listado de citas (server-side) para el DataGrid de la
        /// agenda interna. Usa HttpClient directo (el cliente tipado NSwag no
        /// incluye el endpoint nuevo; regenerarlo exigiria la API corriendo).
        /// </summary>
        public async Task<CitasPaginadasViewModel> GetCitasPaginadas(int pagina, int tamanoPagina)
        {
            var response = await _client.HttpClient
                .GetFromJsonAsync<CitasPaginadasViewModel>(
                    $"api/citas/paginadas?pagina={pagina}&tamanoPagina={tamanoPagina}");

            return response ?? new CitasPaginadasViewModel();
        }

        /// <summary>
        /// Citas del usuario autenticado ("Mis citas"). Usa HttpClient directo
        /// por el mismo motivo: el cliente NSwag aun no incluye el endpoint.
        /// </summary>
        public async Task<List<MiCitaViewModel>> GetMisCitas()
        {
            try
            {
                var citas = await _client.HttpClient
                    .GetFromJsonAsync<List<MiCitaViewModel>>("api/citas/mias");

                return citas ?? [];
            }
            catch (HttpRequestException)
            {
                return [];
            }
        }

        public async Task<CitaDetailViewModel> GetCitaById(Guid id)
        {
            var selectedCita = await _client.GetCitaByIdAsync(id);
            var mappedCita = _mapper.Map<CitaDetailViewModel>(selectedCita);
            return mappedCita;
        }

        public async Task<ApiResponse<Guid>> CreateCita(CitaDetailViewModel eventDetailViewModel)
        {
            try
            {
                CreateCitaCommand createCitaCommand = _mapper.Map<CreateCitaCommand>(eventDetailViewModel);
                var newId = await _client.AddCitaAsync(createCitaCommand);
                return new ApiResponse<Guid>() { Data = newId, Success = true };
            }
            catch (ApiException ex)
            {
                return ConvertApiExceptions<Guid>(ex);
            }
        }

        public async Task<ApiResponse<Guid>> UpdateCita(CitaDetailViewModel eventDetailViewModel)
        {
            try
            {
                UpdateCitaCommand updateCitaCommand = _mapper.Map<UpdateCitaCommand>(eventDetailViewModel);
                await _client.UpdateCitaAsync(updateCitaCommand);
                return new ApiResponse<Guid>() { Success = true };
            }
            catch (ApiException ex)
            {
                return ConvertApiExceptions<Guid>(ex);
            }
        }

        public async Task<ApiResponse<Guid>> DeleteCita(Guid id)
        {
            try
            {
                await _client.DeleteCitaAsync(id);
                return new ApiResponse<Guid>() { Success = true };
            }
            catch (ApiException ex)
            {
                return ConvertApiExceptions<Guid>(ex);
            }
        }
    }
}