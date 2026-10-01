using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    public interface IServicioDataService
    {
        Task<List<ServicioViewModel>> ObtenerTodosAsync();

        Task<ServicioViewModel?> ObtenerPorIdAsync(Guid id);

        Task<ApiResponse<Guid>> CrearAsync(ServicioViewModel servicio);

        Task<ApiResponse> ActualizarAsync(ServicioViewModel servicio);

        Task<ApiResponse> EliminarAsync(Guid id);
    }
}
