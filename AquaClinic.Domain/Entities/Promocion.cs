using AquaClinic.Domain.Common;
using AquaClinic.Domain.Enums;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Promocion de la clinica (ej. descuento por gran inauguracion).
    /// El alcance se define con PromocionesServicios; sin filas asociadas aplica a todos los servicios.
    /// Reservada para la fase de fidelidad: solo esquema por ahora.
    /// </summary>
    public class Promocion : AuditableEntity
    {
        public Promocion()
        {
            PromocionId = Guid.CreateVersion7();
        }

        public Guid PromocionId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public TipoDescuento TipoDescuento { get; set; } = TipoDescuento.Porcentaje;

        /// <summary>Valor del descuento: porcentaje (ej. 15.00 = 15%) o monto en CAD.</summary>
        public decimal Valor { get; set; }

        public DateTimeOffset? VigenciaDesde { get; set; }

        public DateTimeOffset? VigenciaHasta { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<PromocionServicio>? PromocionesServicios { get; set; }
    }
}
