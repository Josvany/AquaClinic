using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Promociones;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosList
{
    public class GetServiciosListQueryHandler : IRequestHandler<GetServiciosListQuery, List<ServicioListVm>>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IPromocionRepository _promocionRepository;
        private readonly IMapper _mapper;

        public GetServiciosListQueryHandler(
            IMapper mapper,
            IServicioRepository servicioRepository,
            IPromocionRepository promocionRepository)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
            _promocionRepository = promocionRepository;
        }

        public async Task<List<ServicioListVm>> Handle(GetServiciosListQuery request, CancellationToken cancellationToken)
        {
            // Solo servicios activos del catalogo, con el nombre de su especialidad
            // (la portada agrupa los banner-cards por especialidad)
            var servicios = (await _servicioRepository.GetActivosConEspecialidadAsync())
                .OrderBy(s => s.Especialidad.Nombre).ThenBy(s => s.Precio)
                .ToList();

            var lista = _mapper.Map<List<ServicioListVm>>(servicios);

            // Descuento vigente de cada servicio. No se acumulan promociones: si a
            // un servicio le alcanzan varias, gana la mas especifica (la por
            // servicio sobre la global); ver ResolucionPromocion.
            var vigentes = await _promocionRepository.GetVigentesAsync(DateTimeOffset.UtcNow);

            if (vigentes.Count == 0)
            {
                return lista;
            }

            foreach (var vm in lista)
            {
                var promocion = ResolucionPromocion.Elegir(vigentes, vm.ServicioId);

                if (promocion is not null)
                {
                    vm.NombrePromocion = promocion.Nombre;
                    vm.TipoDescuentoPromocion = promocion.TipoDescuento;
                    vm.ValorPromocion = promocion.Valor;
                }
            }

            return lista;
        }
    }
}