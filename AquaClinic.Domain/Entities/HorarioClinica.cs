using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Horario de atencion semanal de la clinica (una fila por dia de la semana).
    /// Las horas se expresan en hora local de la clinica (Langley, BC).
    /// </summary>
    public class HorarioClinica : AuditableEntity
    {
        public HorarioClinica()
        {
            HorarioId = Guid.CreateVersion7();
        }

        public Guid HorarioId { get; set; }

        /// <summary>Dia de la semana (0 = domingo ... 6 = s&#225;bado).</summary>
        public DayOfWeek DiaSemana { get; set; }

        public TimeOnly HoraApertura { get; set; }

        public TimeOnly HoraCierre { get; set; }

        public bool Cerrado { get; set; }
    }
}
