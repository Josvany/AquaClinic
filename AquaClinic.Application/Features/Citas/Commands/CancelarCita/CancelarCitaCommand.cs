using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.CancelarCita
{
    /// <summary>
    /// Cancela una cita pendiente o confirmada: pasa el estado a
    /// Cancelada y el horario vuelve a estar disponible para agendar.
    /// </summary>
    public class CancelarCitaCommand : IRequest
    {
        public Guid CitaId { get; set; }
    }
}
