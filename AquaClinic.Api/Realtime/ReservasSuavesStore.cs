using System.Collections.Concurrent;
using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Features.Citas;

namespace AquaClinic.Api.Realtime
{
    /// <summary>
    /// Store en memoria de reservas suaves. Cada conexion SignalR puede sostener
    /// una sola seleccion a la vez; la entrada caduca sola a los 2 minutos si el
    /// usuario no confirma (o se libera antes al cerrar el modal / confirmar).
    /// </summary>
    public class ReservasSuavesStore : IReservasSuavesStore
    {
        private sealed class ReservaSuave
        {
            public required string ConexionId { get; init; }
            public required DateTimeOffset Inicio { get; init; }
            public required DateTimeOffset Fin { get; init; }
            public DateTimeOffset VenceUtc { get; init; }
        }

        public static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

        private readonly ConcurrentDictionary<string, ReservaSuave> _reservas = new();

        public bool IntentarReservar(string conexionId, DateTimeOffset inicio, DateTimeOffset fin)
        {
            LimpiarVencidas();

            var nueva = new ReservaSuave
            {
                ConexionId = conexionId,
                Inicio = inicio,
                Fin = fin,
                VenceUtc = DateTimeOffset.UtcNow.Add(Vigencia)
            };

            lock (_reservas)
            {
                // El slot no debe estar reservado suavemente por otra conexion
                var ocupado = _reservas.Values.Any(r =>
                    r.ConexionId != conexionId && r.Inicio < fin && r.Fin > inicio);

                if (ocupado)
                {
                    return false;
                }

                // La misma conexion reemplaza su seleccion anterior (si cambio de hora)
                _reservas[conexionId] = nueva;
                return true;
            }
        }

        public void LiberarPorConexion(string conexionId)
        {
            _reservas.TryRemove(conexionId, out _);
        }

        public void LiberarPorInicio(DateTimeOffset inicio)
        {
            var objetivo = _reservas.Values
                .FirstOrDefault(r => r.Inicio == inicio);

            if (objetivo is not null)
            {
                _reservas.TryRemove(objetivo.ConexionId, out _);
            }
        }

        public IReadOnlyList<ReservaSuaveDto> ObtenerVigentes()
        {
            LimpiarVencidas();

            lock (_reservas)
            {
                return _reservas.Values
                    .Select(r => new ReservaSuaveDto(r.Inicio, r.Fin))
                    .ToList();
            }
        }

        private void LimpiarVencidas()
        {
            lock (_reservas)
            {
                var ahora = DateTimeOffset.UtcNow;
                var vencidas = _reservas
                    .Where(kvp => kvp.Value.VenceUtc <= ahora)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var clave in vencidas)
                {
                    _reservas.TryRemove(clave, out _);
                }
            }
        }
    }
}
