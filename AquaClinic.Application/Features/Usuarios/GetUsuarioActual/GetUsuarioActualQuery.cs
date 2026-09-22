using MediatR;

namespace AquaClinic.Application.Features.Usuarios.GetUsuarioActual
{
    /// <summary>
    /// Devuelve el perfil con roles del usuario autenticado actual.
    /// Retorna null si el usuario del cookie ya no existe en la BD
    /// (ej. la BD se recreo tras un reinicio de la API).
    /// </summary>
    public class GetUsuarioActualQuery : IRequest<UsuarioVm?>
    {
    }
}
