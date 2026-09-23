using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    /// <summary>
    /// Consultas de agenda para disponibilidad y validacion de citas.
    /// </summary>
    public class AgendaRepository : IAgendaRepository
    {
        private readonly AquaClinicDbContext _dbContext;

        public AgendaRepository(AquaClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<HorarioClinica>> GetHorariosAsync()
        {
            return await _dbContext.HorariosClinica
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Bloqueo>> GetBloqueosClinicaEntreAsync(DateTimeOffset inicio, DateTimeOffset fin)
        {
            return await _dbContext.Bloqueos
                .Where(b => b.PersonalId == null
                            && b.FechaInicio < fin && b.FechaFin > inicio)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Cita>> GetCitasActivasEntreAsync(DateTimeOffset inicio, DateTimeOffset fin)
        {
            return await _dbContext.Citas
                .Where(c => c.Estado != EstadoCita.Cancelada
                            && c.FechaInicio < fin && c.FechaFin > inicio)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
