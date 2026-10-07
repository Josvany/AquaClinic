using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.DataProtection;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Contenido de la sesion que viaja cifrado en la cookie del navegador.
    /// Se guardan SOLO los tokens: el resto del perfil (usuarioId, correo,
    /// nombre y roles) ya viaja como claims dentro del propio access token, asi
    /// que duplicarlo aqui solo engordaria la cookie.
    /// </summary>
    public sealed record SesionCifrada(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiraUtc);

    /// <summary>Respuesta de login/refresh de la API (tokens + perfil).</summary>
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
    /// La sesion del usuario viaja cifrada en una cookie httpOnly: la cookie ES
    /// la sesion. Antes havia un ticket opaco (Guid) y un diccionario estatico en
    /// memoria (SesionPendiente), lo que hacia que cualquier reinicio del
    /// proceso del App borrase TODAS las sesiones y obligara a iniciar sesion de
    /// nuevo aunque el token siguiera vigente. Sin estado en el servidor, la
    /// sesion sobrevive recargas, cierre del navegador y reinicios del App.
    ///
    /// El cifrado lo aporta Data Protection (AES + HMAC). OJO: las claves DEBEN
    /// persistir en disco o la cookie dejaria de descifrarse tras un reinicio;
    /// por defecto ASP.NET las guarda en el perfil del usuario
    /// (%LOCALAPPDATA%\ASP.NET\DataProtection-Keys). En contenedores hay que
    /// montarlas en un volumen (ver AddDataProtection en Program.cs).
    /// </summary>
    public sealed class SesionCookieStore(IDataProtectionProvider dataProtection)
    {
        public const string NombreCookie = "aqua_sesion";

        /// <summary>
        /// Version del proposito de Data Protection. Si cambia el formato del
        /// payload hay que subirla: las cookies viejas dejan de descifrarse y el
        /// usuario vuelve a iniciar sesion una unica vez.
        /// </summary>
        private const string Proposito = "AquaClinic.App.Sesion.v1";

        /// <summary>
        /// Vigencia de la cookie. Debe ser >= la del refresh token de la API
        /// (Jwt:RefreshTokenDias = 7): si la cookie viviera mas, quedaria
        /// huérfana al caducar el refresh token.
        /// </summary>
        public static readonly TimeSpan Vigencia = TimeSpan.FromDays(7);

        private readonly IDataProtector _protector = dataProtection.CreateProtector(Proposito);

        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        /// <summary>Cifra la sesion para poder escribirla en la cookie.</summary>
        public string Cifrar(SesionCifrada sesion) =>
            _protector.Protect(JsonSerializer.Serialize(sesion, _json));

        /// <summary>
        /// Descifra la cookie. Devuelve null si no hay cookie, si vencio, si el
        /// navegador guardo el valor de otra version o si el cifrado no valida:
        /// en todos esos casos se trata como sesion inexistente.
        /// </summary>
        public SesionCifrada? Descifrar(string? valor)
        {
            if (string.IsNullOrEmpty(valor))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<SesionCifrada>(_protector.Unprotect(valor), _json);
            }
            catch
            {
                // Cookie corrupta, de otra version o con firma invalida
                return null;
            }
        }

        /// <summary>Escribe (o reescribe) la cookie de sesion.</summary>
        public void Escribir(HttpResponse response, SesionCifrada sesion, bool esHttps)
        {
            response.Cookies.Append(NombreCookie, Cifrar(sesion), new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                // Secure solo cuando realmente hay HTTPS: en el perfil http de
                // desarrollo (5082) una cookie "secure" no se guardaria.
                Secure = esHttps,
                IsEssential = true,
                Path = "/",
                MaxAge = Vigencia
            });
        }

        /// <summary>Borra la cookie de sesion (logout o sesion no recuperable).</summary>
        public void Borrar(HttpResponse response)
        {
            response.Cookies.Delete(NombreCookie, new CookieOptions { Path = "/" });
        }

        /// <summary>
        /// Correo del titular leido del propio access token (no hace falta
        /// guardarlo en la cookie). Null si el token no se puede parsear.
        /// </summary>
        public static string? LeerEmail(string accessToken)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

                return jwt.Claims.FirstOrDefault(c =>
                    c.Type is System.Security.Claims.ClaimTypes.Email or "email")?.Value
                    ?? jwt.Claims.FirstOrDefault(c =>
                        c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}