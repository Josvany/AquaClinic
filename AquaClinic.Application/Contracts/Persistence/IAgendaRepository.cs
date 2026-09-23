using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// Consultas de agenda para disponibilidad y validacion de citas:
    /// horario semanal de la clinica, bloqueos globales y citas activas.
    /// </summary>
    public interface IAgendaRepository
    {
        Task<IReadOnlyList<HorarioClinica>> GetHorariosAsync();

        /// <summary>Bloqueos de toda la clinica (PersonalId = null) que se solapan con el rango UTC dado.</summary>
        Task<IReadOnlyList<Bloqueo>> GetBloqueosClinicaEntreAsync(DateTimeOffset inicio, DateTimeOffset fin);

        /// <summary>Citas cuyo rango se solapa con el rango UTC dado, excluyendo las canceladas.</summary>
        Task<IReadOnlyList<Cita>> GetCitasActivasEntreAsync(DateTimeOffset inicio, DateTimeOffset fin);
    }
}
