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

        public async Task<bool> IsNombreUnico(string nombre)
        {
            return await _dbContext.Servicios
                .AsNoTracking()
                .AnyAsync(s => s.Nombre == nombre);
        }

        public async Task<IReadOnlyList<Servicio>> GetActivosConEspecialidadAsync()
        {
            return await _dbContext.Servicios
                .AsNoTracking()
                .Include(s => s.Especialidad)
                .Where(s => s.Activo)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Servicio>> GetTodosConEspecialidadAsync()
        {
            return await _dbContext.Servicios
                .AsNoTracking()
                .Include(s => s.Especialidad)
                .ToListAsync();
        }
    }
}
