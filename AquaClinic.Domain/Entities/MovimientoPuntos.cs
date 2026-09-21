using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Movimiento de puntos de fidelidad (acumulo o canje) de un cliente registrado.
    /// El saldo es la suma de los movimientos. Reservado para la fase de fidelidad:
    /// solo esquema por ahora.
    /// </summary>
    public class MovimientoPuntos : AuditableEntity
    {
        public MovimientoPuntos()
        {
            MovimientoId = Guid.CreateVersion7();
        }

        public Guid MovimientoId { get; set; }

        /// <summary>Cliente registrado (Id en AspNetUsers).</summary>
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>Puntos acumulados (+) o canjeados (-).</summary>
        public int Puntos { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public DateTimeOffset Fecha { get; set; }

        /// <summary>Cita que origino el movimiento, si aplica.</summary>
        public Guid? CitaId { get; set; }
    }
}
