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
                .Include(c => c.CitasServicios)!
                .ThenInclude(cs => cs.Servicio)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Cita?> GetByIdWithServiciosAsync(Guid citaId)
        {
            return await _dbContext.Citas
                .Include(c => c.CitasServicios)!
                .ThenInclude(cs => cs.Servicio)
                .ThenInclude(s => s.Especialidad)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CitaId == citaId);
        }

        public async Task<(IReadOnlyList<Cita> Items, int Total)> GetCitasPaginadasAsync(int pagina, int tamanoPagina)
        {
            var query = _dbContext.Citas
                .Include(c => c.CitasServicios)!
                .ThenInclude(cs => cs.Servicio)
                .AsNoTracking();

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.FechaInicio)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IReadOnlyList<Cita>> GetCitasDeUsuarioAsync(string usuarioId)
        {
            return await _dbContext.Citas
                .Include(c => c.CitasServicios)!
                .ThenInclude(cs => cs.Servicio)
                .AsNoTracking()
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaInicio)
                .ToListAsync();
        }

        public async Task<(string? ClienteNombre, string? ClienteEmail, string? PersonalNombre)>
            ObtenerContactoAsync(string? usuarioId, Guid? personalId)
        {
            string? clienteNombre = null;
            string? clienteEmail = null;

            if (!string.IsNullOrEmpty(usuarioId))
            {
                var usuario = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuario is not null)
                {
                    var nombre = $"{usuario.Nombre} {usuario.Apellidos}".Trim();
                    clienteNombre = string.IsNullOrEmpty(nombre) ? null : nombre;
                    clienteEmail = usuario.Email;
                }
            }

            string? personalNombre = null;

            if (personalId.HasValue)
            {
                var personal = await _dbContext.Personal
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PersonalId == personalId.Value);

                if (personal is not null)
                {
                    var nombre = $"{personal.Nombre} {personal.Apellidos}".Trim();
                    personalNombre = string.IsNullOrEmpty(nombre) ? null : nombre;
                }
            }

            return (clienteNombre, clienteEmail, personalNombre);
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
