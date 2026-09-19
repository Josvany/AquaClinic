using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithCitas
{
    public class GetEspecialidadesListWithCitasQueryHandler : IRequestHandler<GetEspecialidadesListWithCitasQuery, List<EspecialidadCitaListVm>>
    {
        private readonly IMapper _mapper;
        private readonly IEspecialidadRepository _especialidadRepository;

        public GetEspecialidadesListWithCitasQueryHandler(IMapper mapper, IEspecialidadRepository especialidadRepository)
        {
            _mapper = mapper;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<List<EspecialidadCitaListVm>> Handle(GetEspecialidadesListWithCitasQuery request, CancellationToken cancellationToken)
        {
            var list = await _especialidadRepository.GetEspecialidadesWithCitas(request.IncluirHistorial);
            return _mapper.Map<List<EspecialidadCitaListVm>>(list);
        }
    }
}
