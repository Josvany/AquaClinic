using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Promociones.Queries.GetPromocionesList
{
    public class GetPromocionesListQueryHandler : IRequestHandler<GetPromocionesListQuery, List<PromocionListVm>>
    {
        private readonly IPromocionRepository _promocionRepository;
        private readonly IMapper _mapper;

        public GetPromocionesListQueryHandler(IMapper mapper, IPromocionRepository promocionRepository)
        {
            _mapper = mapper;
            _promocionRepository = promocionRepository;
        }

        public async Task<List<PromocionListVm>> Handle(GetPromocionesListQuery request, CancellationToken cancellationToken)
        {
            var promociones = await _promocionRepository.GetPromocionesAsync();

            var lista = _mapper.Map<List<PromocionListVm>>(promociones);

            // El alcance (servicios asociados) se copia a mano: AutoMapper no sabe
            // convertir la coleccion de la entidad en una lista de ids.
            for (var i = 0; i < promociones.Count && i < lista.Count; i++)
            {
                lista[i].ServicioIds = promociones[i].PromocionesServicios?
                    .Select(ps => ps.ServicioId)
                    .ToList() ?? [];
            }

            return lista;
        }
    }
}