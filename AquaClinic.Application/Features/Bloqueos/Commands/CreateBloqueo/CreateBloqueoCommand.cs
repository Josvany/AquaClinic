using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Commands.CreateBloqueo
{
    /// <summary>
    /// Crea un bloqueo de TODA la clinica (PersonalId = null): el rango queda
    /// cerrado y no se puede agendar dentro de el.
    /// </summary>
    public class CreateBloqueoCommand : IRequest<Guid>
    {
        public string Motivo { get; set; } = string.Empty;

        /// <summary>Inicio del bloqueo en UTC.</summary>
        public DateTimeOffset FechaInicio { get; set; }

        /// <summary>Fin del bloqueo en UTC (exclusivo).</summary>
        public DateTimeOffset FechaFin { get; set; }

        public override string ToString()
        {
            return $"Bloqueo: {Motivo}; del {FechaInicio:g} al {FechaFin:g}";
        }
    }
}
