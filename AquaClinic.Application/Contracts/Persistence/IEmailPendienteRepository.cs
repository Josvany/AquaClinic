using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// Cola de salida (outbox) de correos de cita con reintentos.
    /// </summary>
    public interface IEmailPendienteRepository
    {
        Task<EmailPendiente> AddAsync(EmailPendiente email);

        Task UpdateAsync(EmailPendiente email);

        /// <summary>
        /// Correos Pendientes cuyo proximo reintento ya vencio
        /// (nunca intentados o con UltimoIntentoUtc anterior al limite).
        /// </summary>
        Task<IReadOnlyList<EmailPendiente>> GetPendientesParaReintentoAsync(int limite, DateTime ultimoIntentoAntesDeUtc);
    }
}
