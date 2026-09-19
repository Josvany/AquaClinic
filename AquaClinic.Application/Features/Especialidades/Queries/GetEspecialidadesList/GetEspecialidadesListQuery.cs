using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList
{
    public class GetEspecialidadesListQuery : IRequest<List<EspecialidadListVm>>
    {
    }
}
