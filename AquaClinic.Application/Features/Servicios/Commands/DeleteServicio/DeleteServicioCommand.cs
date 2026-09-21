using MediatR;

namespace AquaClinic.Application.Features.Servicios.Commands.DeleteServicio
{
    public class DeleteServicioCommand : IRequest
    {
        public Guid ServicioId { get; set; }
    }
}
