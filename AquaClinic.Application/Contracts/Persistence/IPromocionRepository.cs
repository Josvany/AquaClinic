using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// CRUD de promociones para la pantalla administrativa. El alta, la edicion y
    /// la baja usan el CRUD generico de IAsyncRepository; aqui solo van las
    /// consultas del listado y el alcance por servicio.
    /// </summary>
    public interface IPromocionRepository : IAsyncRepository<Promocion>
    {
        /// <summary>Promociones ordenadas por vigencia (las mas recientes primero), con su alcance.</summary>
        Task<IReadOnlyList<Promocion>> GetPromocionesAsync();

        /// <summary>
        /// Promociones activas y dentro de su vigencia en el instante dado, con
        /// sus servicios asociados cargados. Es la entrada de
        /// <see cref="Features.Promociones.ResolucionPromocion"/>.
        /// </summary>
        Task<IReadOnlyList<Promocion>> GetVigentesAsync(DateTimeOffset ahora);

        /// <summary>
        /// Reemplaza el alcance de la promocion por los servicios indicados. Una
        /// lista vacia deja la promocion SIN alcance, que es lo mismo que aplicar
        /// a todos los servicios. Los ids que no existan en el catalogo se ignoran.
        /// </summary>
        Task ReemplazarServiciosAsync(Promocion promocion, IReadOnlyCollection<Guid> servicioIds);
    }
}