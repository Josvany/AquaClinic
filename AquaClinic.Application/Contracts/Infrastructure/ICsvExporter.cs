using AquaClinic.Application.Features.Citas.Queries.GetCitasExport;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosExport;
using System.Collections.Generic;

namespace AquaClinic.Application.Contracts.Infrastructure
{
    public interface ICsvExporter
    {
        byte[] ExportServiciosToCsv(List<ServicioExportDto> servicioExportDtos);

        byte[] ExportCitasToCsv(List<CitaExportDto> citaExportDtos);
    }
}
