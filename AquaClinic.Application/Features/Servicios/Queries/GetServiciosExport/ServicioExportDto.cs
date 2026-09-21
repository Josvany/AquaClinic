namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosExport
{
    public class ServicioExportDto
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
    }
}
