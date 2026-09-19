using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    public class ConsultaRepository : BaseRepository<Consulta>, IConsultaRepository
    {
        public ConsultaRepository(AquaClinicDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Consulta>> GetPagedConsultasForMonth(DateTime date, int page, int size)
        {
            return await _dbContext.Consultas.Where(x => x.FechaSolicitada.Month == date.Month && x.FechaSolicitada.Year == date.Year)
                .Skip((page - 1) * size).Take(size).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetTotalCountOfConsultasForMonth(DateTime date)
        {
            return await _dbContext.Consultas.CountAsync(x => x.FechaSolicitada.Month == date.Month && x.FechaSolicitada.Year == date.Year);
        }
    }
}
