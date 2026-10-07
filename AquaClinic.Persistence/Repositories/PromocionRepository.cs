using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    public class PromocionRepository : BaseRepository<Promocion>, IPromocionRepository
    {
        public PromocionRepository(AquaClinicDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Promocion>> GetPromocionesAsync()
        {
            // Vigencia mas reciente primero; las que no tienen fecha de inicio
            // (vigencia abierta) quedan al final del listado.
            return await _dbContext.Promociones
                .Include(p => p.PromocionesServicios)
                .OrderByDescending(p => p.VigenciaDesde)
                .ThenBy(p => p.Nombre)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Promocion>> GetVigentesAsync(DateTimeOffset ahora)
        {
            return await _dbContext.Promociones
                .Include(p => p.PromocionesServicios)
                .AsNoTracking()
                .Where(p => p.Activo)
                .Where(p => p.VigenciaDesde == null || p.VigenciaDesde <= ahora)
                .Where(p => p.VigenciaHasta == null || p.VigenciaHasta >= ahora)
                .ToListAsync();
        }

        public async Task ReemplazarServiciosAsync(Promocion promocion, IReadOnlyCollection<Guid> servicioIds)
        {
            var deseados = servicioIds.Distinct().ToHashSet();

            var actuales = await _dbContext.PromocionesServicios
                .Where(ps => ps.PromocionId == promocion.PromocionId)
                .ToListAsync();

            // Solo ids que existen hoy en el catalogo: si la UI mando un id viejo
            // (servicio borrado mientras editaba) no debe reventar el guardado.
            var validos = (await _dbContext.Servicios
                    .Where(s => deseados.Contains(s.ServicioId))
                    .Select(s => s.ServicioId)
                    .ToListAsync())
                .ToHashSet();

            // Los que ya no estan selected salen del alcance.
            foreach (var sobrante in actuales.Where(a => !deseados.Contains(a.ServicioId)))
            {
                _dbContext.PromocionesServicios.Remove(sobrante);
            }

            // Los nuevos se agregan (sin duplicar los que ya estaban).
            var yaAsignados = actuales.Select(a => a.ServicioId).ToHashSet();
            foreach (var nuevo in validos.Where(v => !yaAsignados.Contains(v)))
            {
                _dbContext.PromocionesServicios.Add(new PromocionServicio
                {
                    PromocionId = promocion.PromocionId,
                    ServicioId = nuevo
                });
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}