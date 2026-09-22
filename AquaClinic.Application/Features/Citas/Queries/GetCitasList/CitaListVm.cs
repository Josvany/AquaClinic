namespace AquaClinic.Application.Features.Citas.Queries.GetCitasList
{
    /// <summary>
    /// Cita para el listado y el calendario de la App. El contrato (citaId, nombre,
    /// fecha, imagenUrl) es el que consume el cliente tipado de la UI.
    /// </summary>
    public class CitaListVm
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? ImagenUrl { get; set; }
    }
}
