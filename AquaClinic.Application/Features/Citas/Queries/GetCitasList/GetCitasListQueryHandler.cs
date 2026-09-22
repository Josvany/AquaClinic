using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasList
{
    public class GetCitasListQueryHandler : IRequestHandler<GetCitasListQuery, List<CitaListVm>>
    {
        private readonly ICitaRepository _citaRepository;

        public GetCitasListQueryHandler(ICitaRepository citaRepository)
        {
            _citaRepository = citaRepository;
        }

        public async Task<List<CitaListVm>> Handle(GetCitasListQuery request, CancellationToken cancellationToken)
        {
            var citas = await _citaRepository.GetCitasWithServiciosAsync();

            return citas
                .OrderBy(c => c.FechaInicio)
                .Select(c => new CitaListVm
                {
                    CitaId = c.CitaId,
                    Nombre = c.NombreInvitado ?? c.EmailInvitado ?? "Cliente registrado",
                    Fecha = CitaFechas.ALocal(c.FechaInicio),
                    ImagenUrl = c.CitasServicios?.FirstOrDefault()?.Servicio?.ImagenUrl
                })
                .ToList();
        }
    }
}
