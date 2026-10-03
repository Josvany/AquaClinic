using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    public class BloqueoRepository : BaseRepository<Bloqueo>, IBloqueoRepository
    {
        public BloqueoRepository(AquaClinicDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Bloqueo>> GetBloqueosClinicaAsync()
        {
            return await _dbContext.Bloqueos
                .Where(b => b.PersonalId == null)
                .OrderBy(b => b.FechaInicio)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
