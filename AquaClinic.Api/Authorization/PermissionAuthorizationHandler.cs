using Microsoft.AspNetCore.Authorization;

namespace AquaClinic.Api.Authorization
{
    /// <summary>
    /// Valida que el usuario autenticado posea el permiso exigido, consultando
    /// los roles del principal (viajan como claims en la cookie de Identity) y el
    /// mapeo est&#225;tico de <see cref="PermisosPorRol"/>.
    /// </summary>
    internal sealed class PermissionAuthorizationHandler(PermissionProvider permissionProvider)
        : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User is { Identity.IsAuthenticated: true })
            {
                HashSet<string> permissions = permissionProvider.GetForPrincipal(context.User);

                if (permissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
