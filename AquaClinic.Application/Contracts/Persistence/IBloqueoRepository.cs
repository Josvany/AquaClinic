using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// CRUD de bloqueos para la pantalla administrativa. El alta, la edicion y la
    /// baja usan el CRUD generico de IAsyncRepository; aqui solo va la consulta
    /// del listado.
    /// </summary>
    public interface IBloqueoRepository : IAsyncRepository<Bloqueo>
    {
        /// <summary>Bloqueos de toda la clinica (PersonalId = null), por fecha de inicio.</summary>
        Task<IReadOnlyList<Bloqueo>> GetBloqueosClinicaAsync();
    }
}
