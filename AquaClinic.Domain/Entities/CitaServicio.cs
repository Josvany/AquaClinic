namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Relacion N:N entre citas y servicios. Un flujo tipico agrega una sola
    /// fila (un servicio por cita); el esquema permite combinar varios servicios
    /// en la misma visita de forma opcional.
    /// </summary>
    public class CitaServicio
    {
        public Guid CitaId { get; set; }
        public Cita Cita { get; set; } = default!;

        public Guid ServicioId { get; set; }
        public Servicio Servicio { get; set; } = default!;
    }
}
