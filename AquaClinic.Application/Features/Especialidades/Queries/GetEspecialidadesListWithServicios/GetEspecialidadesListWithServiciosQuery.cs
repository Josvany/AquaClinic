using MediatR;
using System.Collections.Generic;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithServicios
{
    public class GetEspecialidadesListWithServiciosQuery : IRequest<List<EspecialidadServicioListVm>>
    {
    }
}
