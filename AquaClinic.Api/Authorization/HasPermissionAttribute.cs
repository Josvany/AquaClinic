using Microsoft.AspNetCore.Authorization;

namespace AquaClinic.Api.Authorization
{
    /// <summary>
    /// Exige un permiso granular (ej. [HasPermission(Permisos.ServiciosCrear)]).
    /// La politica se resuelve dinamicamente con el nombre del permiso.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
            : base(permission)
        {
        }
    }
}