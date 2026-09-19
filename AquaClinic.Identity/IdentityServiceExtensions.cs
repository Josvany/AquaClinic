using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AquaClinic.Identity
{
    public static class IdentityServiceExtensions
    {
        public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

            services.AddAuthorizationBuilder();

            services.AddDbContext<AquaClinicIdentityDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("AquaClinicIdentityConnectionString")));

            services.AddIdentityCore<ApplicationUser>()
                .AddEntityFrameworkStores<AquaClinicIdentityDbContext>()
                .AddApiEndpoints();
        }
    }
}
