using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    public interface ICitaDataService
    {
        Task<List<CitaListViewModel>> GetAllCitas();
        Task<CitasPaginadasViewModel> GetCitasPaginadas(int pagina, int tamanoPagina);
        Task<CitaDetailViewModel> GetCitaById(Guid id);
        Task<ApiResponse<Guid>> CreateCita(CitaDetailViewModel eventDetailViewModel);
        Task<ApiResponse<Guid>> UpdateCita(CitaDetailViewModel eventDetailViewModel);
        Task<ApiResponse<Guid>> DeleteCita(Guid id);
    }
}
