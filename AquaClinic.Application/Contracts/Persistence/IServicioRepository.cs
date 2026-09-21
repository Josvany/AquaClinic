using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface IServicioRepository : IAsyncRepository<Servicio>
    {
        Task<bool> IsNombreUnico(string nombre);
    }
}
