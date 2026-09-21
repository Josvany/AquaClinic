using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;

namespace AquaClinic.Persistence.Repositories
{
    public class ServicioRepository : BaseRepository<Servicio>, IServicioRepository
    {
        public ServicioRepository(AquaClinicDbContext dbContext) : base(dbContext)
        {
        }

        public Task<bool> IsNombreUnico(string nombre)
        {
            var existe = _dbContext.Servicios.Any(s => s.Nombre == nombre);
            return Task.FromResult(existe);
        }
    }
}
