using System.Text;
using AquaClinic.Application.Contracts.Authentication;
using Microsoft.AspNetCore.Http;
using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AquaClinic.Identity
{
    public static class IdentityServiceExtensions
    {
        /// <summary>
        /// Registra la autenticacion JWT contra el contexto unico de la aplicacion
        /// (AquaClinicDbContext, que hereda de IdentityDbContext&lt;ApplicationUser&gt;).
        /// Los roles viajan como claims dentro del access token (emite TokenService),
        /// de modo que PermissionProvider los lee igual que con la cookie de Identity.
        /// La clave y la configuracion provienen de la seccion Jwt (appsettings o
        /// variables de entorno Jwt__Key, Jwt__Issuer, Jwt__Audience).
        /// </summary>
        public static IServiceCollection AddIdentityServices<TDbContext>(this IServiceCollection services, IConfiguration configuration)
            where TDbContext : IdentityDbContext<ApplicationUser>
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"],

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
                                ?? throw new InvalidOperationException(
                                    "Falta la clave JWT (Jwt:Key). Configurala en appsettings (Development) " +
                                    "o por variable de entorno Jwt__Key (produccion)."))),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    // 401 con cuerpo ProblemDetails (el desafio por defecto responde
                    // vacio y el cliente NSwag de la App falla al deserializarlo)
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();

                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/problem+json";

                            await context.Response.WriteAsJsonAsync(new
                            {
                                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                                title = "No autorizado",
                                status = 401,
                                detail = "La sesion no es valida o expiro. Inicia sesion nuevamente."
                            });
                        }
                    };
                });

            services.AddAuthorizationBuilder();

            services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TDbContext>()
                .AddSignInManager()
                .AddApiEndpoints();

            // Operaciones de cuenta (registro, login, cambio de password, restablecimiento)
            services.AddScoped<IIdentityService, IdentityService>();

            // Emision de JWT y gestion de refresh tokens (store estatico compartido)
            services.AddSingleton<TokenService>();

            return services;
        }
    }
}
