namespace AquaClinic.Application.Features.Citas.Queries.GetCitasExport
{
    public class CitaExportDto
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
