using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AquaClinic.Api.IntegrationTests.Base
{
    /// <summary>
    /// Handler de autenticacion simulada: autentica cualquier peticion con un
    /// principal de prueba, para poder probar los endpoints protegidos con [Authorize].
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Rol por defecto Administrador para que los endpoints con
            // [HasPermission] pasen la autorizacion. X-Test-Rol permite probar
            // los permisos de otro rol (por ejemplo, Cliente en /api/citas/mias).
            var rol = Request.Headers["X-Test-Rol"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(rol))
            {
                rol = "Administrador";
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "UsuarioDePrueba"),
                new(ClaimTypes.Role, rol)
            };

            // Permite simular el usuario autenticado actual en los endpoints [Authorize]
            // que dependen del NameIdentifier (por ejemplo, /api/usuarios/actual)
            var usuarioId = Request.Headers["X-Test-UserId"].FirstOrDefault();
            if (!string.IsNullOrEmpty(usuarioId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, usuarioId));
            }

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
