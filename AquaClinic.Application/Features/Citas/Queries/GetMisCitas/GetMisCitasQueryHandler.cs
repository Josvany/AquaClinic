using AquaClinic.Application.Contracts;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetMisCitas
{
    public class GetMisCitasQueryHandler : IRequestHandler<GetMisCitasQuery, List<MiCitaVm>>
    {
        private readonly ICitaRepository _citaRepository;
        private readonly ILoggedInUserService _loggedInUserService;

        public GetMisCitasQueryHandler(ICitaRepository citaRepository, ILoggedInUserService loggedInUserService)
        {
            _citaRepository = citaRepository;
            _loggedInUserService = loggedInUserService;
        }

        public async Task<List<MiCitaVm>> Handle(GetMisCitasQuery request, CancellationToken cancellationToken)
        {
            var usuarioId = _loggedInUserService.UserId;

            if (string.IsNullOrEmpty(usuarioId))
            {
                return [];
            }

            var citas = await _citaRepository.GetCitasDeUsuarioAsync(usuarioId);

            return citas
                .Select(c => new MiCitaVm
                {
                    CitaId = c.CitaId,
                    Servicio = c.CitasServicios?.FirstOrDefault()?.Servicio?.Nombre ?? "Servicio",
                    Precio = c.CitasServicios?.FirstOrDefault()?.Servicio?.Precio ?? 0m,
                    Fecha = CitaFechas.ALocal(c.FechaInicio),
                    Estado = c.Estado.ToString(),
                    Notas = c.Notas
                })
                .ToList();
        }
    }
}
