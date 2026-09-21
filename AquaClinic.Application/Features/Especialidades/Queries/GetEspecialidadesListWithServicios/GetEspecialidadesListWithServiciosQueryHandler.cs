using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithServicios
{
    public class GetEspecialidadesListWithServiciosQueryHandler : IRequestHandler<GetEspecialidadesListWithServiciosQuery, List<EspecialidadServicioListVm>>
    {
        private readonly IMapper _mapper;
        private readonly IEspecialidadRepository _especialidadRepository;

        public GetEspecialidadesListWithServiciosQueryHandler(IMapper mapper, IEspecialidadRepository especialidadRepository)
        {
            _mapper = mapper;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<List<EspecialidadServicioListVm>> Handle(GetEspecialidadesListWithServiciosQuery request, CancellationToken cancellationToken)
        {
            var list = await _especialidadRepository.GetEspecialidadesWithServicios();
            return _mapper.Map<List<EspecialidadServicioListVm>>(list);
        }
    }
}