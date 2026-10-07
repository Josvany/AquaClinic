using MediatR;

namespace AquaClinic.Application.Features.Promociones.Commands.DeletePromocion
{
    /// <summary>Elimina una promocion del catalogo.</summary>
    public class DeletePromocionCommand : IRequest
    {
        public Guid PromocionId { get; set; }
    }
}