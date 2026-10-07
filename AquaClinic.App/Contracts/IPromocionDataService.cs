using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    /// <summary>CRUD de promociones del catalogo para la pantalla administrativa.</summary>
    public interface IPromocionDataService
    {
        Task<List<PromocionViewModel>> ObtenerTodasAsync();

        Task<ApiResponse<Guid>> CrearAsync(PromocionViewModel promocion);

        Task<ApiResponse> ActualizarAsync(PromocionViewModel promocion);

        Task<ApiResponse> EliminarAsync(Guid id);
    }
}