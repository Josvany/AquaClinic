using MediatR;

namespace AquaClinic.Application.Features.Usuarios.GetUsuarioActual
{
    /// <summary>
    /// Devuelve el perfil con roles del usuario autenticado actual.
    /// </summary>
    public class GetUsuarioActualQuery : IRequest<UsuarioVm>
    {
    }
}
