using AquaClinic.Domain.Enums;

namespace AquaClinic.Application.Features.Promociones.Queries.GetPromocionesList
{
    /// <summary>
    /// Promocion del catalogo para la tabla administrativa. La vigencia se guarda
    /// en UTC, como en la base de datos; la App la convierte a hora local de la
    /// clinica al mostrarla y editarla.
    /// </summary>
    public class PromocionListVm
    {
        public Guid PromocionId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public TipoDescuento TipoDescuento { get; set; }

        /// <summary>Porcentaje (ej. 15.00 = 15%) o monto en CAD, segun TipoDescuento.</summary>
        public decimal Valor { get; set; }

        public DateTimeOffset? VigenciaDesde { get; set; }

        public DateTimeOffset? VigenciaHasta { get; set; }

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Servicios a los que aplica. Vacio = promocion global (a todos los
        /// servicios del catalogo).
        /// </summary>
        public List<Guid> ServicioIds { get; set; } = [];

        /// <summary>True cuando no tiene alcance: aplica a todos los servicios.</summary>
        public bool EsGlobal => ServicioIds.Count == 0;
    }
}