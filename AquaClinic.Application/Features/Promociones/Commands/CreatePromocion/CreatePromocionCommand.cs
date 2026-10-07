using AquaClinic.Domain.Enums;
using MediatR;

namespace AquaClinic.Application.Features.Promociones.Commands.CreatePromocion
{
    /// <summary>
    /// Alta de una promocion de la clinica. El descuento se guarda tal cual
    /// (porcentaje o monto en CAD); este CRUD solo administra el catalogo.
    /// </summary>
    public class CreatePromocionCommand : IRequest<Guid>
    {
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
        /// Servicios a los que aplica. Vacio = promotion global (a todos). Los ids
        /// que no existan en el catalogo se ignoran.
        /// </summary>
        public List<Guid> ServicioIds { get; set; } = [];

        public override string ToString()
        {
            return $"Promocion: {Nombre}; {(TipoDescuento == TipoDescuento.Porcentaje ? $"{Valor}%" : $"{Valor} CAD")}";
        }
    }
}