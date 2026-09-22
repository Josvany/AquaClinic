using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    public class CitaRepository : BaseRepository<Cita>, ICitaRepository
    {
        public CitaRepository(AquaClinicDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Cita>> GetCitasWithServiciosAsync()
        {
            return await _dbContext.Citas
                .Include(c => c.CitasServicios)
                .ThenInclude(cs => cs.Servicio)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
