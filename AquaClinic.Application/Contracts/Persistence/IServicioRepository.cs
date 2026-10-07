using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface IServicioRepository : IAsyncRepository<Servicio>
    {
        Task<bool> IsNombreUnico(string nombre);

        /// <summary>Servicios activos del catalogo con su especialidad cargada (portada publica).</summary>
        Task<IReadOnlyList<Servicio>> GetActivosConEspecialidadAsync();

        /// <summary>
        /// Servicios con su especialidad, incluyendo los inactivos (Activo = false).
        /// Es lo que usa la pantalla administrativa para poder listarlos y filtrarlos.
        /// </summary>
        Task<IReadOnlyList<Servicio>> GetTodosConEspecialidadAsync();
    }
}
