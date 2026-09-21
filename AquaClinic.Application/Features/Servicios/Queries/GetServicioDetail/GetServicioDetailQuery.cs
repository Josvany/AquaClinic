using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServicioDetail
{
    public class GetServicioDetailQuery : IRequest<ServicioDetailVm>
    {
        public Guid Id { get; set; }
    }
}
