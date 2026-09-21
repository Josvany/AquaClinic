using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosList
{
    public class GetServiciosListQuery : IRequest<List<ServicioListVm>>
    {
    }
}
