using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Resumen contable agregado de un día×servicio (día contable en hora local
    /// America/Vancouver). Lo recalcula el worker de contabilidad a partir de los
    /// MovimientosServicios: contar día o mes es un SUM sobre esta tabla.
    /// </summary>
    public class ResumenDiarioServicio : AuditableEntity
    {
        public ResumenDiarioServicio()
        {
            ResumenDiarioId = Guid.CreateVersion7();
        }

        public Guid ResumenDiarioId { get; set; }

        /// <summary>Fecha (solo día) del día contable en hora local de la clínica.</summary>
        public DateTime FechaLocal { get; set; }

        public Guid ServicioId { get; set; }
        public Servicio Servicio { get; set; } = default!;

        /// <summary>Cantidad de atenciones registradas ese día para el servicio.</summary>
        public int CantidadAtenciones { get; set; }

        /// <summary>Monto total en CAD del día×servicio (suma de los precios snapshot).</summary>
        public decimal MontoTotal { get; set; }

        /// <summary>Último recálculo en UTC.</summary>
        public DateTime ActualizadoUtc { get; set; }
    }
}
