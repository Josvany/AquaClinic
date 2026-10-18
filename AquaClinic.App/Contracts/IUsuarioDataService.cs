using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using System.Net.Http.Json;

namespace AquaClinic.App.Contracts
{
    /// <summary>
    /// Operaciones de cuenta de usuario contra la API (/api/usuarios).
    /// </summary>
    public interface IUsuarioDataService
    {
        Task<UsuarioViewModel?> LoginAsync(string email, string password, bool emitirCookie);
        Task<UsuarioViewModel?> GetUsuarioActualAsync();

        /// <summary>Clientes registrados (rol Cliente) para agendarles una cita.</summary>
        Task<List<ClienteBusquedaViewModel>> BuscarClientesAsync(string? texto);
        Task<ApiResponse> ActualizarPerfilAsync(string? nombre, string? apellidos, string? telefono);
        Task<ApiResponse> CambiarPasswordAsync(string passwordActual, string passwordNueva);
        Task<ApiResponse<string>> OlvidePasswordAsync(string email);
        Task<ApiResponse> RestablecerPasswordAsync(string email, string token, string passwordNueva);
        Task<ApiResponse> EliminarCuentaAsync();
        Task<ApiResponse<string>> ExportarDatosAsync();
    }
}
