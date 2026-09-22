using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface ICitaRepository : IAsyncRepository<Cita>
    {
        Task<IReadOnlyList<Cita>> GetCitasWithServiciosAsync();
    }
}
