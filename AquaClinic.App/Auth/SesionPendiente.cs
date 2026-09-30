using System.Collections.Concurrent;

namespace AquaClinic.App.Auth
{
    /// <summary>DTO de la respuesta login/refresh de la API (tokens + perfil).</summary>
    public sealed record SesionJwt(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiraUtc,
        UsuarioSesionDto Usuario);

    public sealed record UsuarioSesionDto(
        string UsuarioId,
        string Email,
        string? Nombre,
        string? Apellidos,
        string? Telefono,
        IReadOnlyList<string> Roles);

    /// <summary>
    /// Almacen server-side de sesiones JWT activas, indexado por un ticket opaco
    /// que viaja en una cookie de navegador (httpOnly, sin datos del token).
    /// Asi la sesion sobrevive recargas y navegaciones: cada circuito nuevo
    /// recupera los tokens del servidor con el ticket del navegador.
    /// </summary>
    public static class SesionPendiente
    {
        private static readonly ConcurrentDictionary<string, SesionJwt> _sesiones = new();

        public const string NombreCookie = "aqua_ticket";

        /// <summary>Guarda la sesion emitida por /auth/login y devuelve su ticket.</summary>
        public static string Registrar(SesionJwt sesion)
        {
            var ticket = Guid.CreateVersion7().ToString("N");
            _sesiones[ticket] = sesion;

            // Limpieza de sesiones muy vencidas (mas de 7 dias sin refresh)
            foreach (var vencido in _sesiones
                .Where(kvp => kvp.Value.ExpiraUtc < DateTime.UtcNow.AddDays(-7))
                .Select(kvp => kvp.Key))
            {
                _sesiones.TryRemove(vencido, out _);
            }

            return ticket;
        }

        public static SesionJwt? TomarPorTicket(string? ticket)
        {
            if (string.IsNullOrEmpty(ticket))
            {
                return null;
            }

            return _sesiones.TryGetValue(ticket, out var sesion) ? sesion : null;
        }

        /// <summary>Reemplaza los tokens de una sesion existente (rotacion del refresh).</summary>
        public static void Actualizar(string? ticket, string accessToken, string refreshToken, DateTime expiraUtc)
        {
            if (string.IsNullOrEmpty(ticket))
            {
                return;
            }

            if (_sesiones.TryGetValue(ticket, out var sesion))
            {
                _sesiones[ticket] = sesion with
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiraUtc = expiraUtc
                };
            }
        }

        public static void Eliminar(string? ticket)
        {
            if (!string.IsNullOrEmpty(ticket))
            {
                _sesiones.TryRemove(ticket, out _);
            }
        }
    }
}