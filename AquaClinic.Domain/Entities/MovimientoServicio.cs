using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Movimiento contable de un servicio atendido: una fila inmutable por cada
    /// cita×servicio que se concretó (cita Confirmada o Completada cuya FechaFin
    /// ya pasó). Guarda el precio como snapshot al momento de la atención, de modo
    /// que los cambios posteriores de precio no alteren el historial contable.
    /// La tabla la llena el worker de contabilidad (ContabilidadBackgroundService);
    /// el índice único (CitaId, ServicioId) garantiza que una cita nunca se cuente dos veces.
    /// </summary>
    public class MovimientoServicio : AuditableEntity
    {
        public MovimientoServicio()
        {
            MovimientoServicioId = Guid.CreateVersion7();
        }

        public Guid MovimientoServicioId { get; set; }

        /// <summary>Cita que originó el movimiento.</summary>
        public Guid CitaId { get; set; }
        public Cita Cita { get; set; } = default!;

        public Guid ServicioId { get; set; }
        public Servicio Servicio { get; set; } = default!;

        /// <summary>Personal que atendió (opcional; null = sin asignar).</summary>
        public Guid? PersonalId { get; set; }

        /// <summary>Cliente registrado (AspNetUsers); null cuando la cita fue de invitado.</summary>
        public string? UsuarioId { get; set; }

        /// <summary>Nombre del cliente invitado al momento de la atención; null para usuarios registrados (resolubles por UsuarioId).</summary>
        public string? NombreCliente { get; set; }

        /// <summary>Precio en CAD vigente al momento de la atención (snapshot, no se actualiza).</summary>
        public decimal PrecioSnapshot { get; set; }

        /// <summary>Fecha/hora UTC en que se atendió (FechaFin de la cita).</summary>
        public DateTimeOffset FechaAtencion { get; set; }
    }
}
