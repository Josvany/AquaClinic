using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Proveedor de estado de autenticacion basado en el JWT guardado en el
    /// TokenStore. Los claims (incluidos los roles) se leen directamente del
    /// access token ya validado por la API; no hay consulta al perfil ni cookies.
    /// En SSR estatico el TokenStore puede estar vacio (el circuito aun no
    /// restauro la sesion): entonces se usa el HttpContext.User poblado por
    /// SesionTicketAuthHandler, para que [Authorize] y el nav coincidan.
    /// </summary>
    public class ApiAuthenticationStateProvider(TokenStore tokenStore, IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
    {
        private ClaimsPrincipal? _principal;
        private bool _yaConsultado;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_yaConsultado)
            {
                _principal = ConstruirPrincipal();
                _yaConsultado = true;
            }

            return Task.FromResult(new AuthenticationState(_principal ?? Anonimo()));
        }

        /// <summary>
        /// Reconstruye el principal desde el TokenStore (tras login, refresh o logout).
        /// </summary>
        public void InvalidarCache()
        {
            _principal = ConstruirPrincipal();
            _yaConsultado = true;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private ClaimsPrincipal? ConstruirPrincipal()
        {
            if (!string.IsNullOrEmpty(tokenStore.AccessToken))
            {
                try
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenStore.AccessToken);
                    var identidad = new ClaimsIdentity(jwt.Claims, "Jwt", ClaimTypes.Name, ClaimTypes.Role);
                    return new ClaimsPrincipal(identidad);
                }
                catch
                {
                    // Token corrupto: tratar como sesion terminada
                    tokenStore.Limpiar();
                }
            }

            // SSR estatico sin circuito: usar el usuario reconstruido desde el
            // ticket por el middleware (mismos claims que el JWT)
            return httpContextAccessor.HttpContext?.User;
        }

        private static ClaimsPrincipal Anonimo()
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
