namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosList
{
    public class ServicioListVm
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
        public string? ImagenUrl { get; set; }
    }
}
