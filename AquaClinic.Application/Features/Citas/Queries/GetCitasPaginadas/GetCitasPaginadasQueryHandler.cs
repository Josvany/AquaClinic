using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasPaginadas
{
    public class GetCitasPaginadasQueryHandler : IRequestHandler<GetCitasPaginadasQuery, CitasPaginadasVm>
    {
        private readonly ICitaRepository _citaRepository;

        public GetCitasPaginadasQueryHandler(ICitaRepository citaRepository)
        {
            _citaRepository = citaRepository;
        }

        public async Task<CitasPaginadasVm> Handle(GetCitasPaginadasQuery request, CancellationToken cancellationToken)
        {
            var pagina = Math.Max(1, request.Pagina);
            var tamano = request.TamanoPagina <= 0 ? 10 : request.TamanoPagina;

            var (items, total) = await _citaRepository.GetCitasPaginadasAsync(pagina, tamano);

            return new CitasPaginadasVm
            {
                Pagina = pagina,
                TamanoPagina = tamano,
                Total = total,
                Items = items
                    .Select(c => new CitasPaginadasItemVm
                    {
                        CitaId = c.CitaId,
                        Nombre = c.NombreInvitado ?? c.EmailInvitado ?? "Cliente registrado",
                        Fecha = CitaFechas.ALocal(c.FechaInicio),
                        ImagenUrl = c.CitasServicios?.FirstOrDefault()?.Servicio?.ImagenUrl
                    })
                    .ToList()
            };
        }
    }
}
