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

        public async Task<List<Especialidad>> GetEspecialidadesWithCitas(bool incluirCitasPasadas)
        {
            var allEspecialidades = await _dbContext.Especialidades.Include(x => x.Citas).ToListAsync();
            if (!incluirCitasPasadas)
            {
                allEspecialidades.ForEach(p => p.Citas.ToList().RemoveAll(c => c.Fecha < DateTime.Today));
            }
            return allEspecialidades;
        }
    }
}
