using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// Operaciones de escritura de citas con sus servicios.
    /// </summary>
    public interface ICitaRepository : IAsyncRepository<Cita>
    {
        Task<IReadOnlyList<Cita>> GetCitasWithServiciosAsync();

        /// <summary>Vincula un servicio a la cita (fila en CitaServicio).</summary>
        Task AsignarServicioAsync(Guid citaId, Guid servicioId);
    }
}
