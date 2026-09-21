using MediatR;

namespace AquaClinic.Application.Features.Usuarios.RestablecerPassword
{
    /// <summary>
    /// Restablece la contrase&#241;a usando el token de un solo uso de OlvidePassword.
    /// </summary>
    public class RestablecerPasswordCommand : IRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
    }
}
