using AquaClinic.Domain.Enums;

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

        /// <summary>
        /// Vigente en el catalogo. En el listado publico siempre es true (solo se
        /// exponen los activos); el listado administrativo si incluye los inactivos.
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Promocion vigente que aplica a este servicio (null si ninguna). Solo se
        /// considera una: si hay una global y otra por servicio, gana la
        /// especifica (ver ResolucionPromocion).
        /// </summary>
        public string? NombrePromocion { get; set; }

        /// <summary>Forma del descuento de la promocion vigente.</summary>
        public TipoDescuento? TipoDescuentoPromocion { get; set; }

        /// <summary>Porcentaje (ej. 15.00 = 15%) o monto en CAD, segun TipoDescuentoPromocion.</summary>
        public decimal? ValorPromocion { get; set; }

        /// <summary>
        /// Precio tras aplicar la promocion vigente; igual a Precio cuando no
        /// hay descuento. Nunca baja de 0.
        /// </summary>
        public decimal PrecioConDescuento =>
            ValorPromocion is not decimal valor
                ? Precio
                : Math.Max(0, decimal.Round(
                    TipoDescuentoPromocion == TipoDescuento.Porcentaje
                        ? Precio * (1 - valor / 100m)
                        : Precio - valor, 2));
    }
}