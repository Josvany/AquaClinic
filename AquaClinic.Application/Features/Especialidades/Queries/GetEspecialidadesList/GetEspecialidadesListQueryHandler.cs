using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList
{
    public class GetEspecialidadesListQueryHandler : IRequestHandler<GetEspecialidadesListQuery, List<EspecialidadListVm>>
    {
        private readonly IAsyncRepository<Especialidad> _especialidadRepository;
        private readonly IMapper _mapper;

        public GetEspecialidadesListQueryHandler(IMapper mapper, IAsyncRepository<Especialidad> especialidadRepository)
        {
            _mapper = mapper;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<List<EspecialidadListVm>> Handle(GetEspecialidadesListQuery request, CancellationToken cancellationToken)
        {
            var allEspecialidades = (await _especialidadRepository.ListAllAsync()).OrderBy(x => x.Nombre);
            return _mapper.Map<List<EspecialidadListVm>>(allEspecialidades);
        }
    }
}
