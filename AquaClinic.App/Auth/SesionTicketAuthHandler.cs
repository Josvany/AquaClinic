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
    /// descifra la sesion de la cookie (ver SesionCookieStore), la renueva si el
    /// access token ya vencio y reconstruye el principal con sus claims. Asi el
    /// [Authorize] del middleware (p. ej. /especialidadoverview) y el Challenge
    /// redirigen correctamente sin depender del circuito Blazor.
    ///
    /// No hay estado en el servidor: la sesion viaja cifrada en la cookie, asi
    /// que sobrevive a recargas, al cierre del navegador y a reinicios del App.
    /// </summary>
    public class SesionTicketAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHttpClientFactory httpClientFactory,
        SesionCookieStore sesiones) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var sesion = sesiones.Descifrar(Request.Cookies[SesionCookieStore.NombreCookie]);

            if (sesion is null || string.IsNullOrEmpty(sesion.AccessToken))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                // ReadJwtToken solo parsea: NO valida firma ni vida. El access token
                // vencido no puede autenticar una peticion SSR. Si el refresh token
                // sigue vigente se renueva la sesion en el acto para no expulsar al
                // usuario a login, y la nueva sesion se reescribe en la cookie.
                if (TokenVencido(sesion.AccessToken))
                {
                    sesion = await RenovarSesionAsync(sesion);

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

        /// <summary>
        /// Sin sesion el [Authorize] desafia este esquema: llevar al usuario a
        /// /login en vez del 401 vacio (que dejaba la pagina en blanco; el
        /// esquema no tiene LoginPath porque no emite cookies de sesion).
        /// </summary>
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Redirect("/login");
            return Task.CompletedTask;
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
        /// Rota el refresh token contra la API y reescribe la cookie con la sesion
        /// renovada. Devuelve la sesion nueva, o null si el refresh ya no es
        /// valido y el usuario debe re-autenticar.
        /// </summary>
        private async Task<SesionCifrada?> RenovarSesionAsync(SesionCifrada sesion)
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
                    // Refresh realmente invalido (vencido, revocado o porque la API
                    // reinicio y perdio sus refresh tokens en memoria): la sesion se
                    // termina y se limpia la cookie para no reintentar en cada request.
                    Logger.LogWarning(
                        "Refresh token rechazado: la sesion se cierra (posible API reiniciada o token vencido).");
                    sesiones.Borrar(Response);
                    return null;
                }

                var datos = await respuesta.Content.ReadFromJsonAsync<RespuestaRefresh>();

                if (datos is null || string.IsNullOrEmpty(datos.AccessToken))
                {
                    return null;
                }

                var renovada = new SesionCifrada(datos.AccessToken, datos.RefreshToken, datos.ExpiraUtc);

                // La rotacion es de un solo uso: si la cookie se queda con el token
                // viejo, el siguiente refresh fallaria. Reescribirla aqui es lo que
                // mantiene la sesion viva entre peticiones SSR.
                sesiones.Escribir(Response, renovada, Request.IsHttps);

                var tokenStore = Context.RequestServices.GetService<TokenStore>();
                tokenStore?.ActualizarAccessToken(renovada.AccessToken, renovada.ExpiraUtc);
                tokenStore?.GuardarRefresh(renovada.RefreshToken);

                return renovada;
            }
            catch
            {
                // API no disponible: no autenticar con el token vencido
                return null;
            }
        }

        private sealed record RespuestaRefresh(
            string AccessToken,
            string RefreshToken,
            DateTime ExpiraUtc);
    }
}