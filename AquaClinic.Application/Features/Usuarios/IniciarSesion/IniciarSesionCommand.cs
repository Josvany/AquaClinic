using MediatR;

namespace AquaClinic.Application.Features.Usuarios.IniciarSesion
{
    /// <summary>
    /// Valida credenciales y devuelve el perfil con roles del usuario
    /// (null si son inv&#225;lidas; la emision de cookie es responsabilidad del endpoint).
    /// </summary>
    public class IniciarSesionCommand : IRequest<UsuarioVm?>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
