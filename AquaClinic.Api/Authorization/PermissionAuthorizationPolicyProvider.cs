using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AquaClinic.Api.Authorization
{
    /// <summary>
    /// Crea politicas din&#225;micamente para cualquier permiso usado con
    /// [HasPermission(...)], sin tener que registrarlas una por una.
    /// </summary>
    internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

            if (policy is not null)
            {
                return policy;
            }

            AuthorizationPolicy permissionPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return permissionPolicy;
        }
    }
}
