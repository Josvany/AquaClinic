using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IReadOnlyList<Servicio>> GetActivosConEspecialidadAsync()
        {
            return await _dbContext.Servicios
                .Include(s => s.Especialidad)
                .Where(s => s.Activo)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Servicio>> GetTodosConEspecialidadAsync()
        {
            return await _dbContext.Servicios
                .Include(s => s.Especialidad)
                .ToListAsync();
        }
    }
}
