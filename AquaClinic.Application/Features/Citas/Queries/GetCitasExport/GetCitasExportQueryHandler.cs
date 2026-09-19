using AutoMapper;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasExport
{
    public class GetCitasExportQueryHandler : IRequestHandler<GetCitasExportQuery, CitaExportFileVm>
    {
        private readonly IAsyncRepository<Cita> _citaRepository;
        private readonly IMapper _mapper;
        private readonly ICsvExporter _csvExporter;

        public GetCitasExportQueryHandler(IMapper mapper, IAsyncRepository<Cita> citaRepository, ICsvExporter csvExporter)
        {
            _mapper = mapper;
            _citaRepository = citaRepository;
            _csvExporter = csvExporter;
        }

        public async Task<CitaExportFileVm> Handle(GetCitasExportQuery request, CancellationToken cancellationToken)
        {
            var allCitas = _mapper.Map<List<CitaExportDto>>((await _citaRepository.ListAllAsync()).OrderBy(x => x.Fecha));

            var fileData = _csvExporter.ExportCitasToCsv(allCitas);

            var eventExportFileDto = new CitaExportFileVm() { ContentType = "text/csv", Data = fileData, CitaExportFileName = $"{Guid.NewGuid()}.csv" };

            return eventExportFileDto;
        }
    }
}
