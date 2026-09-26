using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// Operaciones de escritura de citas con sus servicios.
    /// </summary>
    public interface ICitaRepository : IAsyncRepository<Cita>
    {
        Task<IReadOnlyList<Cita>> GetCitasWithServiciosAsync();

        /// <summary>
        /// Pagina de citas ordenadas por fecha descendente (para el listado
        /// server-side de la agenda interna). Devuelve (items, total).
        /// </summary>
        Task<(IReadOnlyList<Cita> Items, int Total)> GetCitasPaginadasAsync(int pagina, int tamanoPagina);

        /// <summary>Vincula un servicio a la cita (fila en CitaServicio).</summary>
        Task AsignarServicioAsync(Guid citaId, Guid servicioId);
    }
}
