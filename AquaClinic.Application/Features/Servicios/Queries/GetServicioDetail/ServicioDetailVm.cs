namespace AquaClinic.Application.Features.Servicios.Queries.GetServicioDetail
{
    public class ServicioDetailVm
    {
        public Guid ServicioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int DuracionMinutos { get; set; }

        public string? Descripcion { get; set; }

        public string? ImagenUrl { get; set; }

        public Guid EspecialidadId { get; set; }

        public EspecialidadDto Especialidad { get; set; } = default!;
    }
}
