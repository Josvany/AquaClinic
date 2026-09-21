using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Bloqueo de agenda: dia festivo, cierre, capacitacion o vacaciones.
    /// Puede ser de toda la clinica (PersonalId = null) o de una persona en particular.
    /// Rango en UTC (datetimeoffset).
    /// </summary>
    public class Bloqueo : AuditableEntity
    {
        public Bloqueo()
        {
            BloqueoId = Guid.CreateVersion7();
        }

        public Guid BloqueoId { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public DateTimeOffset FechaInicio { get; set; }

        public DateTimeOffset FechaFin { get; set; }

        /// <summary>Personal bloqueado; null = toda la clinica.</summary>
        public Guid? PersonalId { get; set; }

        public Personal? Personal { get; set; }
    }
}
