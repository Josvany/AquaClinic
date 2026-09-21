using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Proveedor de estado de autenticacion que consulta el perfil del usuario
    /// contra la API (GET /api/usuarios/actual). El CookieForwardingHandler
    /// reenvia la cookie del navegador durante SSR/prerender; el perfil se
    /// cachea por la duracion del circuito para evitar llamadas repetidas.
    /// No requiere Identity local ni acceso directo a la base de datos.
    /// </summary>
    public class ApiAuthenticationStateProvider(HttpClient httpClient) : AuthenticationStateProvider
    {
        private UsuarioPerfil? _perfil;
        private bool _yaConsultado = false;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Solo consulta la API la primera vez (o despues de invalidar cache)
            if (_yaConsultado)
            {
                return _perfil is not null ? ConstruirEstado(_perfil) : Anonimo();
            }

            try
            {
                var perfil = await httpClient.GetFromJsonAsync<UsuarioPerfil>(
                    "api/usuarios/actual", CancellationToken.None);

                if (perfil is not null && !string.IsNullOrEmpty(perfil.Email))
                {
                    _perfil = perfil;
                    _yaConsultado = true;
                    return ConstruirEstado(perfil);
                }
            }
            catch
            {
                // API no disponible o 401: usuario no autenticado
            }

            _perfil = null;
            _yaConsultado = true;
            return Anonimo();
        }

        /// <summary>
        /// Limpia la cache despues de login/logout para forzar una nueva consulta.
        /// </summary>
        public void InvalidarCache()
        {
            _perfil = null;
            _yaConsultado = false;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private static AuthenticationState Anonimo()
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        private static AuthenticationState ConstruirEstado(UsuarioPerfil perfil)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, perfil.UsuarioId),
                new(ClaimTypes.Name, perfil.Email),
                new(ClaimTypes.Email, perfil.Email)
            };

            if (!string.IsNullOrEmpty(perfil.Nombre))
            {
                claims.Add(new Claim("nombre", perfil.Nombre));
            }

            if (!string.IsNullOrEmpty(perfil.Apellidos))
            {
                claims.Add(new Claim("apellidos", perfil.Apellidos));
            }

            foreach (var rol in perfil.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var identity = new ClaimsIdentity(claims, "ApiCookie");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // DTO interno para deserializar la respuesta de /api/usuarios/actual
        private record UsuarioPerfil(
            string UsuarioId,
            string Email,
            string? Nombre,
            string? Apellidos,
            string? Telefono,
            List<string> Roles);
    }
}
