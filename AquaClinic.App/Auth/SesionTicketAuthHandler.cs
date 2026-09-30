using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
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
        UrlEncoder encoder,
        IHttpClientFactory httpClientFactory) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var ticket = Request.Cookies[SesionPendiente.NombreCookie];
            var sesion = SesionPendiente.TomarPorTicket(ticket);

            if (sesion is null || string.IsNullOrEmpty(sesion.AccessToken))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                // ReadJwtToken solo parsea: NO valida firma ni vida. El access token
                // vencido no puede autenticar una peticion SSR (antes quedaba valido
                // hasta 7 dias, la vida del ticket). Si el refresh token sigue vigente
                // se renueva la sesion en el acto para no expulsar al usuario a login.
                if (TokenVencido(sesion.AccessToken))
                {
                    sesion = await RenovarSesionAsync(ticket!, sesion);

                    if (sesion is null)
                    {
                        return AuthenticateResult.NoResult();
                    }
                }

                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(sesion.AccessToken);
                var identidad = new ClaimsIdentity(jwt.Claims, "Jwt", ClaimTypes.Name, ClaimTypes.Role);
                var principal = new ClaimsPrincipal(identidad);
                var resultado = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(resultado);
            }
            catch
            {
                // Token corrupto: tratar como anonimo
                return AuthenticateResult.NoResult();
            }
        }

        private static bool TokenVencido(string accessToken)
        {
            try
            {
                return new JwtSecurityTokenHandler().ReadJwtToken(accessToken).ValidTo <= DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Rota el refresh token contra la API, persiste la rotacion en la sesion
        /// del ticket y en el TokenStore del circuito (si hay uno montado en este
        /// request). Devuelve la sesion renovada, o null si el refresh ya no es
        /// valido y el usuario debe re-autenticar.
        /// </summary>
        private async Task<SesionJwt?> RenovarSesionAsync(string ticket, SesionJwt sesion)
        {
            try
            {
                using var client = httpClientFactory.CreateClient("ApiAuth");
                using var peticion = new HttpRequestMessage(HttpMethod.Post, "api/usuarios/refresh")
                {
                    Content = JsonContent.Create(new { refreshToken = sesion.RefreshToken })
                };

                using var respuesta = await client.SendAsync(peticion);

                if (!respuesta.IsSuccessStatusCode)
                {
                    // Carrera benigna: otra peticion concurrente ya roto el mismo
                    // refresh token. Si la sesion guardada tiene tokens nuevos se
                    // sigue con ella en vez de expulsar al usuario.
                    var actual = SesionPendiente.TomarPorTicket(ticket);
                    if (actual is not null
                        && !string.IsNullOrEmpty(actual.AccessToken)
                        && !string.Equals(actual.RefreshToken, sesion.RefreshToken, StringComparison.Ordinal))
                    {
                        return actual;
                    }

                    // Refresh realmente invalido: la sesion expiro de verdad
                    SesionPendiente.Eliminar(ticket);
                    return null;
                }

                var datos = await respuesta.Content.ReadFromJsonAsync<SesionJwt>();

                if (datos is null || string.IsNullOrEmpty(datos.AccessToken))
                {
                    return null;
                }

                // Persistir la rotacion: los siguientes requests SSR (y el circuito
                // que se monte con este ticket) usan los tokens nuevos.
                SesionPendiente.Actualizar(ticket, datos.AccessToken, datos.RefreshToken, datos.ExpiraUtc);

                var tokenStore = Context.RequestServices.GetService<TokenStore>();
                tokenStore?.ActualizarAccessToken(datos.AccessToken, datos.ExpiraUtc);
                tokenStore?.GuardarRefresh(datos.RefreshToken);

                return datos;
            }
            catch
            {
                // API no disponible: no autenticar con el token vencido
                return null;
            }
        }
    }
}
