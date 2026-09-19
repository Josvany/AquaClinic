namespace AquaClinic.Application.Features.Citas.Queries.GetCitasList
{
    public class CitaListVm
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
    }
}