using AquaClinic.App.Services;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    public interface IEspecialidadDataService
    {
        Task<List<EspecialidadViewModel>> GetAllEspecialidades();
        Task<List<EspecialidadCitasViewModel>> GetAllEspecialidadesWithCitas(bool incluirHistorial);
        Task<ApiResponse<EspecialidadDto>> CreateEspecialidad(EspecialidadViewModel especialidadViewModel);
    }
}
