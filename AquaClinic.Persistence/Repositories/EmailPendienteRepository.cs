using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    public class EmailPendienteRepository : IEmailPendienteRepository
    {
        private readonly AquaClinicDbContext _dbContext;

        public EmailPendienteRepository(AquaClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<EmailPendiente> AddAsync(EmailPendiente email)
        {
            await _dbContext.EmailsPendientes.AddAsync(email);
            await _dbContext.SaveChangesAsync();
            return email;
        }

        public async Task UpdateAsync(EmailPendiente email)
        {
            _dbContext.EmailsPendientes.Update(email);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<EmailPendiente>> GetPendientesParaReintentoAsync(int limite, DateTime ultimoIntentoAntesDeUtc)
        {
            return await _dbContext.EmailsPendientes
                .Where(e => e.Estado == EstadoEmail.Pendiente
                    && (e.UltimoIntentoUtc == null || e.UltimoIntentoUtc < ultimoIntentoAntesDeUtc))
                .OrderBy(e => e.UltimoIntentoUtc ?? DateTime.MinValue)
                .Take(limite)
                .ToListAsync();
        }
    }
}
