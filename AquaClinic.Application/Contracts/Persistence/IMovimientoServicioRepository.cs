using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface IMovimientoServicioRepository
    {
        /// <summary>
        /// Citas elegibles para contabilidad: solo aceptadas (Confirmada o Completada)
        /// cuya FechaFin ya paso y que aun no tengan movimiento registrado.
        /// Incluye sus servicios y precios para el snapshot.
        /// </summary>
        Task<IReadOnlyList<Cita>> GetCitasParaRegistrarAsync(DateTimeOffset ahoraUtc);

        /// <summary>Agrega movimientos y guarda cambios en una sola transaccion.</summary>
        Task AgregarMovimientosAsync(IEnumerable<MovimientoServicio> movimientos);

        /// <summary>Resumen diario de un servicio (o null si aun no existe).</summary>
        Task<ResumenDiarioServicio?> GetResumenAsync(DateTime fechaLocal, Guid servicioId);

        /// <summary>Inserta o actualiza el resumen diario.</summary>
        Task GuardarResumenAsync(ResumenDiarioServicio resumen);

        /// <summary>Movimientos cuya FechaAtencion cae en la ventana UTC [desde, hasta).</summary>
        Task<IReadOnlyList<MovimientoServicio>> GetMovimientosEntreAsync(
            DateTimeOffset desdeUtc, DateTimeOffset hastaUtc);
    }
}
