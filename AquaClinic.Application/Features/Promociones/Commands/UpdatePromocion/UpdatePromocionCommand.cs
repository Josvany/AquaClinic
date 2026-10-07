using AquaClinic.Domain.Enums;
using MediatR;

namespace AquaClinic.Application.Features.Promociones.Commands.UpdatePromocion
{
    /// <summary>Modifica los datos y la vigencia de una promocion existente.</summary>
    public class UpdatePromocionCommand : IRequest
    {
        public Guid PromocionId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        /// <summary>Porcentaje (1) o monto fijo en CAD (2).</summary>
        public TipoDescuento TipoDescuento { get; set; } = TipoDescuento.Porcentaje;

        /// <summary>Porcentaje (ej. 15.00 = 15%) o monto en CAD, segun TipoDescuento.</summary>
        public decimal Valor { get; set; }

        /// <summary>Inicio de la vigencia en UTC; null = sin limite.</summary>
        public DateTimeOffset? VigenciaDesde { get; set; }

        /// <summary>Fin de la vigencia en UTC; null = sin limite.</summary>
        public DateTimeOffset? VigenciaHasta { get; set; }

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Servicios a los que aplica. Vacio = promocion global (a todos). Los ids
        /// que no existan en el catalogo se ignoran.
        /// </summary>
        public List<Guid> ServicioIds { get; set; } = [];
    }
}