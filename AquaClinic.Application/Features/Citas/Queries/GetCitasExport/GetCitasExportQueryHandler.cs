using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasExport
{
    public class GetCitasExportQueryHandler : IRequestHandler<GetCitasExportQuery, CitaExportFileVm>
    {
        private readonly ICitaRepository _citaRepository;
        private readonly ICsvExporter _csvExporter;

        public GetCitasExportQueryHandler(ICitaRepository citaRepository, ICsvExporter csvExporter)
        {
            _citaRepository = citaRepository;
            _csvExporter = csvExporter;
        }

        public async Task<CitaExportFileVm> Handle(GetCitasExportQuery request, CancellationToken cancellationToken)
        {
            var citas = await _citaRepository.GetCitasWithServiciosAsync();

            var registros = citas
                .OrderBy(c => c.FechaInicio)
                .Select(c => new CitaExportDto
                {
                    CitaId = c.CitaId,
                    Nombre = c.NombreInvitado ?? c.EmailInvitado ?? "Cliente registrado",
                    Fecha = CitaFechas.ALocal(c.FechaInicio),
                    Servicio = c.CitasServicios?.FirstOrDefault()?.Servicio?.Nombre
                })
                .ToList();

            var fileData = _csvExporter.ExportCitasToCsv(registros);

            return new CitaExportFileVm
            {
                ContentType = "text/csv",
                Data = fileData,
                CitaExportFileName = $"{Guid.NewGuid()}.csv"
            };
        }
    }
}
