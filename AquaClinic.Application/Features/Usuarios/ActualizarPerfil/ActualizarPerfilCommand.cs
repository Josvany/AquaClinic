using MediatR;

namespace AquaClinic.Application.Features.Usuarios.ActualizarPerfil
{
    /// <summary>
    /// Actualiza el perfil del usuario autenticado actual.
    /// </summary>
    public class ActualizarPerfilCommand : IRequest
    {
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Telefono { get; set; }
    }
}
