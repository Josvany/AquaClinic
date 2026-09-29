using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    public class MovimientoServicioRepository : IMovimientoServicioRepository
    {
        private readonly AquaClinicDbContext _dbContext;

        public MovimientoServicioRepository(AquaClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Cita>> GetCitasParaRegistrarAsync(DateTimeOffset ahoraUtc)
        {
            // Unico punto de verdad del filtro: solo citas aceptadas (Confirmada o
            // Completada) cuya FechaFin ya paso y que aun no tengan movimiento.
            return await _dbContext.Citas
                .Include(c => c.CitasServicios)!
                .ThenInclude(cs => cs.Servicio)
                .AsNoTracking()
                .Where(c => (c.Estado == EstadoCita.Confirmada || c.Estado == EstadoCita.Completada)
                    && c.FechaFin <= ahoraUtc
                    && !_dbContext.MovimientosServicios.Any(m => m.CitaId == c.CitaId))
                .OrderBy(c => c.FechaFin)
                .ToListAsync();
        }

        public async Task AgregarMovimientosAsync(IEnumerable<MovimientoServicio> movimientos)
        {
            await _dbContext.MovimientosServicios.AddRangeAsync(movimientos);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ResumenDiarioServicio?> GetResumenAsync(DateTime fechaLocal, Guid servicioId)
        {
            return await _dbContext.ResumenesDiarios
                .FirstOrDefaultAsync(r => r.FechaLocal == fechaLocal && r.ServicioId == servicioId);
        }

        public async Task GuardarResumenAsync(ResumenDiarioServicio resumen)
        {
            if (_dbContext.Entry(resumen).State == EntityState.Detached)
            {
                _dbContext.ResumenesDiarios.Add(resumen);
            }
            else
            {
                _dbContext.ResumenesDiarios.Update(resumen);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<MovimientoServicio>> GetMovimientosEntreAsync(
            DateTimeOffset desdeUtc, DateTimeOffset hastaUtc)
        {
            return await _dbContext.MovimientosServicios
                .AsNoTracking()
                .Where(m => m.FechaAtencion >= desdeUtc && m.FechaAtencion < hastaUtc)
                .OrderBy(m => m.FechaAtencion)
                .ToListAsync();
        }
    }
}
