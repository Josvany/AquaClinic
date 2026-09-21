using MediatR;

namespace AquaClinic.Application.Features.Usuarios.CambiarPassword
{
    /// <summary>
    /// Cambia la contrase&#241;a del usuario autenticado actual, verificando la vigente.
    /// </summary>
    public class CambiarPasswordCommand : IRequest
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
    }
}
