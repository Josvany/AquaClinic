using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    /// <summary>CRUD de bloqueos de la clinica para la pantalla administrativa.</summary>
    public interface IBloqueoDataService
    {
        Task<List<BloqueoViewModel>> ObtenerTodosAsync();

        Task<ApiResponse<Guid>> CrearAsync(BloqueoViewModel bloqueo);

        Task<ApiResponse> ActualizarAsync(BloqueoViewModel bloqueo);

        Task<ApiResponse> EliminarAsync(Guid id);
    }
}
