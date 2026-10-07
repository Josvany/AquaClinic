using AquaClinic.Api.Authorization;
using AquaClinic.Api.Infrastructure;
using AquaClinic.Application.Features.Usuarios;
using AquaClinic.Application.Features.Usuarios.BuscarClientes;
using AquaClinic.Application.Features.Usuarios.ActualizarPerfil;
using AquaClinic.Application.Features.Usuarios.CambiarPassword;
using AquaClinic.Application.Features.Usuarios.GetUsuarioActual;
using AquaClinic.Application.Features.Usuarios.IniciarSesion;
using AquaClinic.Application.Features.Usuarios.OlvidePassword;
using AquaClinic.Application.Features.Usuarios.Registrarse;
using AquaClinic.Application.Features.Usuarios.RestablecerPassword;
using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AquaClinic.Api.Controllers
{
    /// <summary>
    /// Cuenta del cliente: registro, inicio de sesion (con cookie opcional),
    /// perfil, cambio y restablecimiento de contrase&#241;a.
    /// Los endpoints que manipulan credenciales usan la politica estricta de
    /// rate limiting "authentication".
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly TokenService _tokenService;
        private readonly IIdentityService _identityService;

        public UsuariosController(IMediator mediator, TokenService tokenService, IIdentityService identityService)
        {
            _mediator = mediator;
            _tokenService = tokenService;
            _identityService = identityService;
        }

        /// <summary>
        /// Clientes registrados (rol Cliente) para agendarles una cita desde el back
        /// office: filtra por nombre, apellidos o correo. Solo datos de contacto.
        /// </summary>
        [HttpGet("clientes", Name = "BuscarClientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.CitasGestionar)]
        public async Task<ActionResult<List<ClienteBusquedaVm>>> BuscarClientes(
            [FromQuery] string? texto,
            [FromQuery] int limite = 50)
        {
            var result = await _mediator.Send(new BuscarClientesQuery { Texto = texto, Limite = limite });
            return Ok(result);
        }

        /// <summary>Registra un cliente nuevo (rol Cliente).</summary>
        [HttpPost("registro", Name = "RegistrarUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [EnableRateLimiting(RateLimitingPolicies.Authentication)]
        public async Task<ActionResult<string>> Registrar([FromBody] RegistrarseCommand command)
        {
            var usuarioId = await _mediator.Send(command);
            return Ok(usuarioId);
        }

        /// <summary>
        /// Valida credenciales y devuelve un access token JWT (con los roles como
        /// claims) mas un refresh token rotable, junto con el perfil del usuario.
        /// </summary>
        [HttpPost("login", Name = "IniciarSesion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        [EnableRateLimiting(RateLimitingPolicies.Authentication)]
        public async Task<ActionResult<RespuestaLoginVm>> IniciarSesion([FromBody] IniciarSesionCommand command)
        {
            var usuarioVm = await _mediator.Send(command);

            if (usuarioVm is null)
            {
                return Unauthorized();
            }

            return Ok(await EmitirSesionAsync(usuarioVm));
        }

        /// <summary>
        /// Renueva la sesion: valida el refresh token (un solo uso), rota el token
        /// y emite un nuevo access token JWT con los roles actuales del usuario.
        /// </summary>
        [HttpPost("refresh", Name = "RefrescarSesion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        [EnableRateLimiting(RateLimitingPolicies.Authentication)]
        public async Task<ActionResult<RespuestaLoginVm>> Refrescar([FromBody] PeticionRefresh peticion)
        {
            var usuarioId = await _tokenService.ValidarYRotarRefreshTokenAsync(peticion.RefreshToken);

            if (usuarioId is null)
            {
                return Unauthorized();
            }

            var usuario = await _identityService.ObtenerPorIdAsync(usuarioId);

            if (usuario is null)
            {
                return Unauthorized();
            }

            return Ok(await EmitirSesionAsync(new Application.Features.Usuarios.UsuarioVm
            {
                UsuarioId = usuario.UsuarioId,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Telefono = usuario.Telefono,
                Roles = usuario.Roles.ToList()
            }));
        }

        /// <summary>Revoca los refresh tokens del usuario autenticado (logout).</summary>
        [HttpPost("logout", Name = "CerrarSesion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<ActionResult> CerrarSesion()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(usuarioId))
            {
                await _tokenService.RevocarRefreshTokensAsync(usuarioId);
            }

            return NoContent();
        }

        /// <summary>
        /// Emite la sesion completa (access token JWT + refresh token persistido).
        /// Antes era sincrona y usaba GetAwaiter().GetResult() sobre el JWT; con el
        /// refresh token en BD eso bloquearia un hilo esperando I/O, asi que ahora
        /// es async de verdad.
        /// </summary>
        private async Task<RespuestaLoginVm> EmitirSesionAsync(
            Application.Features.Usuarios.UsuarioVm usuario)
        {
            var (token, expira) = await _tokenService
                .EmitirAccessTokenAsync(
                    usuario.UsuarioId, usuario.Email, usuario.Roles, usuario.Nombre);

            var refreshToken = await _tokenService.EmitirRefreshTokenAsync(usuario.UsuarioId);

            return new RespuestaLoginVm(
                token,
                refreshToken,
                expira,
                usuario);
        }

        /// <summary>Perfil del usuario autenticado actual. 401 si el cookie referencia un usuario que ya no existe.</summary>
        [HttpGet("actual", Name = "GetUsuarioActual")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<ActionResult<UsuarioVm>> GetUsuarioActual()
        {
            var usuario = await _mediator.Send(new GetUsuarioActualQuery());

            if (usuario is null)
            {
                // El cookie es valido criptograficamente pero el usuario ya no existe
                // en la BD (ej. se recreo tras reinicio). Responde 401 para que
                // el cliente sepa que debe re-autenticar.
                return Unauthorized();
            }

            return Ok(usuario);
        }

        /// <summary>Actualiza el perfil del usuario autenticado actual.</summary>
        [HttpPut("actual", Name = "ActualizarPerfilUsuario")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<ActionResult> ActualizarPerfil([FromBody] ActualizarPerfilCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>Cambia la contrase&#241;a del usuario autenticado actual.</summary>
        [HttpPost("cambiar-password", Name = "CambiarPassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        [Authorize]
        [EnableRateLimiting(RateLimitingPolicies.Authentication)]
        public async Task<ActionResult> CambiarPassword([FromBody] CambiarPasswordCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Solicita el restablecimiento de contrase&#241;a. Responde aceptado siempre;
        /// fuera de Production el token de un solo uso se incluye en la respuesta
        /// (en Production llega &#250;nicamente por correo).
        /// </summary>
        [HttpPost("olvide-password", Name = "OlvidePassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [EnableRateLimiting(RateLimitingPolicies.Authentication)]
        public async Task<ActionResult<OlvidePasswordResponse>> OlvidePassword([FromBody] OlvidePasswordCommand command)
        {
            var respuesta = await _mediator.Send(command);

            var token = HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsProduction()
                ? null
                : respuesta.Token;

            return Ok(new OlvidePasswordResponse(respuesta.SolicitudAceptada, token));
        }

        /// <summary>Restablece la contrase&#241;a con el token de un solo uso.</summary>
        [HttpPost("restablecer-password", Name = "RestablecerPassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        [EnableRateLimiting(RateLimitingPolicies.Authentication)]
        public async Task<ActionResult> RestablecerPassword([FromBody] RestablecerPasswordCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }
    }

    /// <summary>Respuesta de login/refresh con JWT y refresh token rotable.</summary>
    public record RespuestaLoginVm(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiraUtc,
        UsuarioVm Usuario);

    /// <summary>Peticion de renovacion de sesion.</summary>
    public record PeticionRefresh(string RefreshToken);
}
