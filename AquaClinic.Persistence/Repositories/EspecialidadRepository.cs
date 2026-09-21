using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    public class EspecialidadRepository : BaseRepository<Especialidad>, IEspecialidadRepository
    {
        public EspecialidadRepository(AquaClinicDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Especialidad>> GetEspecialidadesWithServicios()
        {
            return await _dbContext.Especialidades
                .Include(e => e.Servicios)
                .ToListAsync();
        }
    }
}
