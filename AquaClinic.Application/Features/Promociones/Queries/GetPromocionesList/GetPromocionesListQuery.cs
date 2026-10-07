using MediatR;

namespace AquaClinic.Application.Features.Promociones.Queries.GetPromocionesList
{
    /// <summary>
    /// Todas las promociones del catalogo para la tabla administrativa,
    /// ordenadas por vigencia (las mas recientes primero).
    /// </summary>
    public class GetPromocionesListQuery : IRequest<List<PromocionListVm>>
    {
    }
}