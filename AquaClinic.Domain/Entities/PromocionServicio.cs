namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Relacion N:N entre promociones y los servicios a los que aplican.
    /// Una promocion sin filas asociadas aplica a todos los servicios.
    /// </summary>
    public class PromocionServicio
    {
        public Guid PromocionId { get; set; }
        public Promocion Promocion { get; set; } = default!;

        public Guid ServicioId { get; set; }
        public Servicio Servicio { get; set; } = default!;
    }
}
