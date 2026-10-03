using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Commands.UpdateBloqueo
{
    /// <summary>Modifica el motivo y el rango de un bloqueo existente.</summary>
    public class UpdateBloqueoCommand : IRequest
    {
        public Guid BloqueoId { get; set; }

        public string Motivo { get; set; } = string.Empty;

        /// <summary>Inicio del bloqueo en UTC.</summary>
        public DateTimeOffset FechaInicio { get; set; }

        /// <summary>Fin del bloqueo en UTC (exclusivo).</summary>
        public DateTimeOffset FechaFin { get; set; }
    }
}
