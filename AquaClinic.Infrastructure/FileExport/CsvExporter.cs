using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosExport;

namespace AquaClinic.Infrastructure.FileExport
{
    public class CsvExporter : ICsvExporter
    {
        public byte[] ExportServiciosToCsv(List<ServicioExportDto> servicioExportDtos)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
                using var csvWriter = new CsvWriter(streamWriter, csvConfig);
                csvWriter.WriteRecords(servicioExportDtos);
            }

            return memoryStream.ToArray();
        }
    }
}
