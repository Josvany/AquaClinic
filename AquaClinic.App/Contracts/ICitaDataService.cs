using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    public interface ICitaDataService
    {
        Task<List<CitaListViewModel>> GetAllCitas();
        Task<CitasPaginadasViewModel> GetCitasPaginadas(int pagina, int tamanaPagina);

        /// <summary>Citas del usuario autenticado (pantalla "Mis citas").</summary>
        Task<List<MiCitaViewModel>> GetMisCitas();

        /// <summary>Detalle real de una cita (cliente, servicios, fecha y estado).</summary>
        Task<CitaDetalleViewModel?> ObtenerDetalleAsync(Guid id);

        /// <summary>Mueve la cita a otro horario; falla (409) si el horario ya fue tomado.</summary>
        Task<ApiResponse<Guid>> MoverCitaAsync(Guid citaId, DateTime nuevoInicioLocal, int duracionMinutos);

        /// <summary>Cancela la cita (solo pendientes o confirmadas).</summary>
        Task<ApiResponse<string>> CancelarCitaAsync(Guid citaId);
    }
}
