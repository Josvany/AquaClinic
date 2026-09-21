using System.Security.Claims;

namespace AquaClinic.Api.Authorization
{
    /// <summary>
    /// Obtiene los permisos de un usuario a partir de los roles de su principal.
    /// Los roles est&#225;n disponibles como claims porque la cookie de Identity los
    /// incluye (AddRoles en el registro de Identity), sin consultas a la BD.
    /// </summary>
    public class PermissionProvider
    {
        public HashSet<string> GetForPrincipal(ClaimsPrincipal principal)
        {
            var roles = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            return PermisosPorRol.Obtener(roles);
        }
    }
}
