namespace AquaClinic.Application.Features.Citas.Queries.GetCitasExport
{
    public class CitaExportFileVm
    {
        public string CitaExportFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[]? Data { get; set; }
    }
}