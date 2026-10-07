using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Citas;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
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

        public async Task<(IReadOnlyList<Cita> Items, int Total)> GetCitasPaginadasAsync(
            int pagina, int tamanaPagina, string? texto = null, string? estado = null,
            DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _dbContext.Citas
                .AsNoTracking()
                .AsQueryable();

            // Filtro de estado (nombre del enum; vacio = todas)
            if (!string.IsNullOrWhiteSpace(estado)
                && Enum.TryParse<EstadoCita>(estado, ignoreCase: true, out var estadoFiltro))
            {
                query = query.Where(c => c.Estado == estadoFiltro);
            }

            // Filtro de texto: cliente invitado (nombre/correo) o
            // registrado (nombre, apellidos o correo de AspNetUsers)
            if (!string.IsNullOrWhiteSpace(texto))
            {
                var limpio = texto.Trim();

                query = query.Where(c =>
                    (c.NombreInvitado != null && c.NombreInvitado.Contains(limpio)) ||
                    (c.EmailInvitado != null && c.EmailInvitado.Contains(limpio)) ||
                    (c.UsuarioId != null && _dbContext.Users.Any(u =>
                        u.Id == c.UsuarioId && (
                            (u.Nombre != null && u.Nombre.Contains(limpio)) ||
                            (u.Apellidos != null && u.Apellidos.Contains(limpio)) ||
                            (u.Email != null && u.Email.Contains(limpio))))));
            }

            // Filtro de fecha en hora local de la clinica: "desde" es
            // inclusivo y "hasta" abarca el dia completo (se compara contra
            // las 00:00 del dia siguiente).
            if (fechaDesde.HasValue || fechaHasta.HasValue)
            {
                var desde = fechaDesde.HasValue ? CitaFechas.AUtc(fechaDesde.Value.Date) : (DateTimeOffset?)null;
                var hasta = fechaHasta.HasValue ? CitaFechas.AUtc(fechaHasta.Value.Date.AddDays(1)) : (DateTimeOffset?)null;

                if (desde.HasValue && hasta.HasValue)
                {
                    var limiteInferior = desde.Value;
                    var limiteSuperior = hasta.Value;
                    query = query.Where(c => c.FechaInicio >= limiteInferior && c.FechaInicio < limiteSuperior);
                }
                else if (desde.HasValue)
                {
                    var limiteInferior = desde.Value;
                    query = query.Where(c => c.FechaInicio >= limiteInferior);
                }
                else
                {
                    var limiteSuperior = hasta!.Value;
                    query = query.Where(c => c.FechaInicio < limiteSuperior);
                }
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.FechaInicio)
                .Skip((pagina - 1) * tamanaPagina)
                .Take(tamanaPagina)
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
