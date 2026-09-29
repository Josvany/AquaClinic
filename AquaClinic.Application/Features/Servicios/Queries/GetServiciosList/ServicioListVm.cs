namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosList
{
    public class ServicioListVm
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
        public string? ImagenUrl { get; set; }

        /// <summary>Nombre de la especialidad a la que pertenece (para agrupar en la portada).</summary>
        public string? EspecialidadNombre { get; set; }
    }
}
