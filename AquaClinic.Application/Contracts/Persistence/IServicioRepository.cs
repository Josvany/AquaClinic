using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface IServicioRepository : IAsyncRepository<Servicio>
    {
        Task<bool> IsNombreUnico(string nombre);

        /// <summary>Servicios activos del catalogo con su especialidad cargada (portada publica).</summary>
        Task<IReadOnlyList<Servicio>> GetActivosConEspecialidadAsync();
    }
}
