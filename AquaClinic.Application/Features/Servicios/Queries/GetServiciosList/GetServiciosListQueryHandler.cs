using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosList
{
    public class GetServiciosListQueryHandler : IRequestHandler<GetServiciosListQuery, List<ServicioListVm>>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IMapper _mapper;

        public GetServiciosListQueryHandler(IMapper mapper, IServicioRepository servicioRepository)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
        }

        public async Task<List<ServicioListVm>> Handle(GetServiciosListQuery request, CancellationToken cancellationToken)
        {
            var allServicios = (await _servicioRepository.ListAllAsync()).OrderBy(x => x.Nombre);
            return _mapper.Map<List<ServicioListVm>>(allServicios);
        }
    }
}
