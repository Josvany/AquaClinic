using AutoMapper;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosExport
{
    public class GetServiciosExportQueryHandler : IRequestHandler<GetServiciosExportQuery, ServicioExportFileVm>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IMapper _mapper;
        private readonly ICsvExporter _csvExporter;

        public GetServiciosExportQueryHandler(IMapper mapper, IServicioRepository servicioRepository, ICsvExporter csvExporter)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
            _csvExporter = csvExporter;
        }

        public async Task<ServicioExportFileVm> Handle(GetServiciosExportQuery request, CancellationToken cancellationToken)
        {
            var allServicios = _mapper.Map<List<ServicioExportDto>>((await _servicioRepository.ListAllAsync()).OrderBy(x => x.Nombre));

            var fileData = _csvExporter.ExportServiciosToCsv(allServicios);

            var servicioExportFileDto = new ServicioExportFileVm() { ContentType = "text/csv", Data = fileData, ServicioExportFileName = $"{Guid.NewGuid()}.csv" };

            return servicioExportFileDto;
        }
    }
}
