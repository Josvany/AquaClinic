using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface IEspecialidadRepository : IAsyncRepository<Especialidad>
    {
        Task<List<Especialidad>> GetEspecialidadesWithServicios();
    }
}
