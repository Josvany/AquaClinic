using MediatR;
using System.Collections.Generic;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithCitas
{
    public class GetEspecialidadesListWithCitasQuery : IRequest<List<EspecialidadCitaListVm>>
    {
        public bool IncluirHistorial { get; set; }
    }
}
