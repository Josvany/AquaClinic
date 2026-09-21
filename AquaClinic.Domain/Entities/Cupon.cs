using AquaClinic.Domain.Common;
using AquaClinic.Domain.Enums;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Cupon asignado a un cliente (tipicamente como regalia por fidelidad).
    /// Reservado para la fase de fidelidad: solo esquema por ahora.
    /// </summary>
    public class Cupon : AuditableEntity
    {
        public Cupon()
        {
            CuponId = Guid.CreateVersion7();
        }

        public Guid CuponId { get; set; }

        public Guid PromocionId { get; set; }
        public Promocion Promocion { get; set; } = default!;

        /// <summary>Cliente al que se asigno el cupon; null = cupon gen&#233;rico.</summary>
        public string? UsuarioId { get; set; }

        /// <summary>Codigo &#250;nico de canje (opcional).</summary>
        public string? Codigo { get; set; }

        public EstadoCupon Estado { get; set; } = EstadoCupon.Activo;

        public DateTimeOffset? FechaAsignacion { get; set; }

        public DateTimeOffset? FechaCanje { get; set; }
    }
}
