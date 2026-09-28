using AquaClinic.Domain.Common;
using AquaClinic.Domain.Enums;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Correo de cita registrado en la cola de salida (outbox). Se persiste
    /// ANTES de intentar el envio: si Mailjet falla, el correo queda Pendiente
    /// y el servicio de reintentos lo reenvia hasta confirmarlo (o agotar los
    /// intentos, quedando Fallido para auditoria). La cita nunca falla por un
    /// problema de correo.
    /// </summary>
    public class EmailPendiente : AuditableEntity
    {
        public EmailPendiente()
        {
            EmailPendienteId = Guid.CreateVersion7();
        }

        public Guid EmailPendienteId { get; set; }

        /// <summary>Cita que origino el correo (trazabilidad; opcional).</summary>
        public Guid? CitaId { get; set; }

        public string Destinatario { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Cuerpo { get; set; } = string.Empty;

        public EstadoEmail Estado { get; set; } = EstadoEmail.Pendiente;

        /// <summary>Cantidad de intentos de envio realizados.</summary>
        public int Intentos { get; set; }

        /// <summary>Ultimo intento en UTC; null = nunca se intento.</summary>
        public DateTime? UltimoIntentoUtc { get; set; }

        public DateTime? EnviadoUtc { get; set; }

        public string? UltimoError { get; set; }
    }
}
