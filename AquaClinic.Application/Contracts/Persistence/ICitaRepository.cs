using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// Operaciones de escritura de citas con sus servicios.
    /// </summary>
    public interface ICitaRepository : IAsyncRepository<Cita>
    {
        Task<IReadOnlyList<Cita>> GetCitasWithServiciosAsync();

        /// <summary>Cita por Id con su servicio y especialidad (detalle de la agenda).</summary>
        Task<Cita?> GetByIdWithServiciosAsync(Guid citaId);

        /// <summary>
        /// Pagina de citas ordenadas por fecha descendente (para el listado
        /// server-side de la agenda interna), con filtros opcionales de
        /// texto (cliente), estado y rango de fechas. Devuelve (items, total).
        /// </summary>
        Task<(IReadOnlyList<Cita> Items, int Total)> GetCitasPaginadasAsync(int pagina, int tamanaPagina, string? texto = null, string? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);

        /// <summary>Vincula un servicio a la cita (fila en CitaServicio).</summary>
        Task AsignarServicioAsync(Guid citaId, Guid servicioId);

        /// <summary>Citas de un usuario registrado, mas recientes primero, con su servicio.</summary>
        Task<IReadOnlyList<Cita>> GetCitasDeUsuarioAsync(string usuarioId);

        /// <summary>
        /// Nombre y correo del cliente registrado (AspNetUsers) y nombre
        /// del personal asignado; null cuando no hay. Para el detalle de
        /// la cita (la entidad Cita no navea a esos registros).
        /// </summary>
        Task<(string? ClienteNombre, string? ClienteEmail, string? PersonalNombre)>
            ObtenerContactoAsync(string? usuarioId, Guid? personalId);
    }
}
