using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasList
{
    public class GetCitasListQuery : IRequest<List<CitaListVm>>
    {
    }
}
