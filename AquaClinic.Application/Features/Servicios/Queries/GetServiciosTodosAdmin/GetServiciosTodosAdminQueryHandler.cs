using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosList;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosTodosAdmin
{
    public class GetServiciosTodosAdminQueryHandler
        : IRequestHandler<GetServiciosTodosAdminQuery, List<ServicioListVm>>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IMapper _mapper;

        public GetServiciosTodosAdminQueryHandler(IMapper mapper, IServicioRepository servicioRepository)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
        }

        public async Task<List<ServicioListVm>> Handle(
            GetServiciosTodosAdminQuery request, CancellationToken cancellationToken)
        {
            // Activos primero y, dentro de cada grupo, por precio: al mezclar los
            // inactivos queda claro que los ultimos son los dados de baja.
            var servicios = (await _servicioRepository.GetTodosConEspecialidadAsync())
                .OrderByDescending(s => s.Activo)
                .ThenBy(s => s.Especialidad?.Nombre ?? string.Empty)
                .ThenBy(s => s.Precio)
                .ToList();

            return _mapper.Map<List<ServicioListVm>>(servicios);
        }
    }
}