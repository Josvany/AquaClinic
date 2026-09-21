using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AquaClinic.Identity
{
    public static class IdentityServiceExtensions
    {
        /// <summary>
        /// Registra la autenticacion por cookies de ASP.NET Identity contra el
        /// contexto &#250;nico de la aplicacion (AquaClinicDbContext, que hereda de
        /// IdentityDbContext<ApplicationUser>). Los roles quedan registrados con
        /// AddRoles para que viajen como claims en la cookie.
        /// </summary>
        public static IServiceCollection AddIdentityServices<TDbContext>(this IServiceCollection services)
            where TDbContext : IdentityDbContext<ApplicationUser>
        {
            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();

            services.AddAuthorizationBuilder();

            services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TDbContext>()
                .AddSignInManager()
                .AddApiEndpoints();

            // Operaciones de cuenta (registro, login, cambio de password, restablecimiento)
            services.AddScoped<IIdentityService, IdentityService>();

            return services;
        }
    }
}
