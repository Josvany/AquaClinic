using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface IEspecialidadRepository : IAsyncRepository<Especialidad>
    {
        Task<List<Especialidad>> GetEspecialidadesWithServicios();

        /// <summary>
        /// True si la especialidad tiene al menos un servicio asignado. Bloquea el
        /// borrado: dejaria servicios huerfanos.
        /// </summary>
        Task<bool> TieneServiciosAsync(Guid especialidadId);
    }
}
