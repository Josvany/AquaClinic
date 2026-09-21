using MediatR;

namespace AquaClinic.Application.Features.Usuarios.OlvidePassword
{
    /// <summary>
    /// Respuesta de la solicitud de restablecimiento. El token solo se expone
    /// fuera de Production para poder completar el flujo en desarrollo sin
    /// SendGrid configurado; en Production llega &#250;nicamente por correo.
    /// </summary>
    public record OlvidePasswordResponse(bool SolicitudAceptada, string? Token = null);

    /// <summary>
    /// Solicita el restablecimiento de contrase&#241;a para un correo. Siempre responde
    /// aceptado (no revela si el correo existe) y envia el token por correo.
    /// </summary>
    public class OlvidePasswordCommand : IRequest<OlvidePasswordResponse>
    {
        public string Email { get; set; } = string.Empty;
    }
}
