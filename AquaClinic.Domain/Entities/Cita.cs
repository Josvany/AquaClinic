using AquaClinic.Domain.Common;
using AquaClinic.Domain.Enums;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Cita agendada por un cliente. Los usuarios registrados se vinculan por
    /// UsuarioId; los invitados pueden agendar dejando sus datos de contacto.
    /// Las fechas se almacenan en UTC (datetimeoffset) para manejar correctamente
    /// los cambios de horario (PST/PDT) de la Columbia Brit&#225;nica.
    /// </summary>
    public class Cita : AuditableEntity
    {
        public Cita()
        {
            CitaId = Guid.CreateVersion7();
        }

        public Guid CitaId { get; set; }

        /// <summary>Id del usuario registrado (AspNetUsers); null cuando el cliente es invitado.</summary>
        public string? UsuarioId { get; set; }

        // Datos de contacto para clientes invitados (o de respaldo)
        public string? NombreInvitado { get; set; }
        public string? EmailInvitado { get; set; }
        public string? TelefonoInvitado { get; set; }

        /// <summary>Personal asignado (opcional; null = por asignar).</summary>
        public Guid? PersonalId { get; set; }

        public DateTimeOffset FechaInicio { get; set; }

        public DateTimeOffset FechaFin { get; set; }

        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

        public string? Notas { get; set; }

        public ICollection<CitaServicio>? CitasServicios { get; set; }
    }
}
