using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    /// <summary>
    /// CRUD de especialidades para la pantalla administrativa.
    ///
    /// OJO: antes esta interfaz usaba el cliente generado por NSwag
    /// (Services/Base/ServiceClient.cs). Ese cliente esta DESACTUALIZado (pide
    /// api/especialidades/allwithcitas, una ruta que ya no existe y devuelve 404
    /// con cuerpo vacio), lo que reventaba la pagina con
    /// "The input does not contain any JSON tokens". Aqui se habla directo por
    /// HTTP, como ya hacen BloqueoDataService, ServicioDataService y
    /// PromocionDataService.
    /// </summary>
    public interface IEspecialidadDataService
    {
        /// <summary>Todas las especialidades, con el numero de servicios de cada una.</summary>
        Task<List<EspecialidadViewModel>> ObtenerTodasAsync();

        /// <summary>Alta. Devuelve el id de la especialidad creada.</summary>
        Task<ApiResponse<Guid>> CrearAsync(EspecialidadViewModel especialidad);

        /// <summary>Cambia el nombre de una especialidad.</summary>
        Task<ApiResponse> ActualizarAsync(EspecialidadViewModel especialidad);

        /// <summary>
        /// Elimina una especialidad. La API responde 400 si tiene servicios
        /// asignados.
        /// </summary>
        Task<ApiResponse> EliminarAsync(Guid id);
    }
}