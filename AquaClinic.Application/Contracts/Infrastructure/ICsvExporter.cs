using AquaClinic.Application.Features.Citas.Queries.GetCitasExport;
using System.Collections.Generic;

namespace AquaClinic.Application.Contracts.Infrastructure
{
    public interface ICsvExporter
    {
        byte[] ExportCitasToCsv(List<CitaExportDto> citaExportDtos);
    }
}
