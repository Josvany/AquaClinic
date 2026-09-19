using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;

namespace AquaClinic.Persistence.Repositories
{
    public class CitaRepository : BaseRepository<Cita>, ICitaRepository
    {
        public CitaRepository(AquaClinicDbContext dbContext) : base(dbContext)
        {
        }

        public Task<bool> IsCitaNombreYFechaUnico(string name, DateTime citaFecha)
        {
            var matches = _dbContext.Citas.Any(e => e.Nombre.Equals(name) && e.Fecha.Date.Equals(citaFecha.Date));
            return Task.FromResult(matches);
        }
    }
}
