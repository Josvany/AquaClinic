using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.DeleteCita
{
    public class DeleteCitaCommand : IRequest
    {
        public Guid CitaId { get; set; }
    }
}
