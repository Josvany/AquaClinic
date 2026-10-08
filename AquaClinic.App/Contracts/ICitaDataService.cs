using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    public interface ICitaDataService
    {
        Task<List<CitaListViewModel>> GetAllCitas();

        /// <summary>
        /// Pagina del listado server-side con filtros opcionales
        /// de texto (cliente), estado y rango de fechas.
        /// </summary>
        Task<CitasPaginadasViewModel> GetCitasPaginadas(int pagina, int tamanaPagina, string? texto = null, string? estado = null, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null);

        /// <summary>Citas del usuario autenticado (pantalla "Mis citas").</summary>
        Task<List<MiCitaViewModel>> GetMisCitas();

        /// <summary>Detalle real de una cita (cliente, servicios, fecha y estado).</summary>
        Task<CitaDetalleViewModel?> ObtenerDetalleAsync(Guid id);

        /// <summary>Mueve la cita a otro horario; falla (409) si el horario ya fue tomado.</summary>
        Task<ApiResponse<Guid>> MoverCitaAsync(Guid citaId, DateTime nuevoInicioLocal, int duracionMinutos);

        /// <summary>
        /// Reprograma una cita propia desde "Mis citas"
        /// (PUT api/citas/mias/{id}/mover). La API valida que la cita
        /// pertenezca al usuario autenticado.
        /// </summary>
        Task<ApiResponse<Guid>> MoverMisCitaAsync(Guid citaId, DateTime nuevoInicioLocal, int duracionMinutos);

        /// <summary>Cancela la cita (solo pendientes o confirmadas).</summary>
        Task<ApiResponse<string>> CancelarCitaAsync(Guid citaId);

        /// <summary>
        /// Cancela una cita propia desde "Mis citas"
        /// (POST api/citas/mias/{id}/cancelar).
        /// </summary>
        Task<ApiResponse<string>> CancelarMisCitaAsync(Guid citaId);
    }
}
