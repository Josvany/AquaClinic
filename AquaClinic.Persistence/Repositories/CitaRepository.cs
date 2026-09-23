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

        public async Task AsignarServicioAsync(Guid citaId, Guid servicioId)
        {
            var existe = await _dbContext.CitasServicios
                .AnyAsync(cs => cs.CitaId == citaId && cs.ServicioId == servicioId);

            if (!existe)
            {
                _dbContext.CitasServicios.Add(new CitaServicio
                {
                    CitaId = citaId,
                    ServicioId = servicioId
                });
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
