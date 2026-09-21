using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServicioDetail
{
    public class GetServicioDetailQueryHandler : IRequestHandler<GetServicioDetailQuery, ServicioDetailVm>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IEspecialidadRepository _especialidadRepository;
        private readonly IMapper _mapper;

        public GetServicioDetailQueryHandler(
            IMapper mapper,
            IServicioRepository servicioRepository,
            IEspecialidadRepository especialidadRepository)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<ServicioDetailVm> Handle(GetServicioDetailQuery request, CancellationToken cancellationToken)
        {
            var servicio = await _servicioRepository.GetByIdAsync(request.Id);
            var servicioDetailDto = _mapper.Map<ServicioDetailVm>(servicio);

            var especialidad = await _especialidadRepository.GetByIdAsync(servicio.EspecialidadId);

            servicioDetailDto.Especialidad = _mapper.Map<EspecialidadDto>(especialidad);

            return servicioDetailDto;
        }
    }
}
