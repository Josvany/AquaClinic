namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosExport
{
    public class ServicioExportFileVm
    {
        public string ServicioExportFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[]? Data { get; set; }
    }
}
