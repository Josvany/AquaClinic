using AquaClinic.Application.Features.Citas;

namespace AquaClinic.Application.Contracts.Citas
{
    /// <summary>
    /// Almacen de reservas suaves (slots seleccionados que aun no se confirman).
    /// Vive en memoria en la API; su contenido se difunde por SignalR para que
    /// todos los visitantes vean la hora "en proceso". Las entradas caducan
    /// solas (~2 minutos) si el usuario no confirma la cita.
    /// </summary>
    public interface IReservasSuavesStore
    {
        /// <summary>Intenta reservar suavemente un rango. Devuelve false si ya esta ocupado o reservado.</summary>
        bool IntentarReservar(string conexionId, DateTimeOffset inicio, DateTimeOffset fin);

        /// <summary>Libera la reserva suave asociada a una conexion (modal cerrado o cita confirmada).</summary>
        void LiberarPorConexion(string conexionId);

        /// <summary>Libera la reserva suave cuyo rango inicia en la fecha dada.</summary>
        void LiberarPorInicio(DateTimeOffset inicio);

        /// <summary>Reservas vigentes (no caducadas).</summary>
        IReadOnlyList<ReservaSuaveDto> ObtenerVigentes();
    }
}
