using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Esquema de autenticacion de la App para las peticiones HTTP (SSR estatico):
    /// reconstruye el principal desde la sesion JWT del servidor (ticket opaco en
    /// cookie) igual que lo hace TokenStoreInitializer para el circuito interactivo.
    /// Asi el [Authorize] del middleware (p. ej. /especialidadoverview) y el
    /// Challenge redirigen correctamente sin depender del circuito Blazor.
    /// </summary>
    public class SesionTicketAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var ticket = Request.Cookies[SesionPendiente.NombreCookie];
            var sesion = SesionPendiente.TomarPorTicket(ticket);

            if (sesion is null || string.IsNullOrEmpty(sesion.AccessToken))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(sesion.AccessToken);
                var identidad = new ClaimsIdentity(jwt.Claims, "Jwt", ClaimTypes.Name, ClaimTypes.Role);
                var principal = new ClaimsPrincipal(identidad);
                var resultado = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(resultado));
            }
            catch
            {
                // Token corrupto: tratar como anonimo
                return Task.FromResult(AuthenticateResult.NoResult());
            }
        }
    }
}
