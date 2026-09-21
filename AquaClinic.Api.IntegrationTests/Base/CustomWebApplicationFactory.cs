using AquaClinic.Identity;
using AquaClinic.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Api.IntegrationTests.Base
{
    /// <summary>
    /// Factory base para tests de integracion de la API. Usa el entorno "Testing"
    /// (appsettings.Testing.json trae limites de rate limiting altos para no
    /// agotarse entre metodos de test), autenticacion simulada (TestAuthHandler)
    /// y base de datos InMemory.
    /// </summary>
    public class CustomWebApplicationFactory<TProgram>
            : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Entorno dedicado a pruebas: evita que el Program de la API reinicie la base de datos
            // y carga appsettings.Testing.json (rate limits altos)
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Esquema de autenticacion simulada que sobreescribe el esquema
                // por defecto de la aplicacion (necesario para los endpoints [Authorize])
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });

                // Elimina el registro de SQL Server del contexto real y lo reemplaza
                // por un contexto InMemory para las pruebas
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AquaClinicDbContext>));
                services.AddDbContext<AquaClinicDbContext>(options =>
                {
                    options.UseInMemoryDatabase("AquaClinicDbContextInMemoryTest");
                });
            });
        }

        /// <summary>
        /// Prepara la base de datos de pruebas. EnsureDeleted + EnsureCreated da
        /// a cada metodo de test una BD InMemory limpia; luego siembra roles y datos.
        /// </summary>
        public void SeedDatabase()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AquaClinicDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.ChangeTracker.Clear();

            // Roles de Identity (idempotente contra la BD)
            var rolesBD = context.Roles.ToList();
            var faltantes = RolesClinica.Todos
                .Where(rol => !rolesBD.Any(r => r.NormalizedName == rol.ToUpperInvariant()))
                .ToList();

            if (faltantes.Count > 0)
            {
                foreach (var rol in faltantes)
                {
                    context.Roles.Add(new IdentityRole(rol)
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        NormalizedName = rol.ToUpperInvariant()
                    });
                }
                context.SaveChanges();
            }

            Utilities.InitializeDbForTests(context);
        }

        public HttpClient GetAnonymousClient()
        {
            return CreateClient();
        }
    }

    /// <summary>
    /// Factory especifico para el test de rate limiting: hereda del base (Testing env,
    /// InMemory, TestAuthHandler) y RE-REGISTRA el rate limiter con limites BAJOS
    /// (10/60s auth) para poder verificar el 429, deshaciendo los limites altos
    /// de appsettings.Testing.json.
    /// </summary>
    public class RateLimitTestWebApplicationFactory<TProgram>
            : CustomWebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Hereda el entorno Testing (InMemory, TestAuthHandler, sin ResetDatabase)
            base.ConfigureWebHost(builder);

            // Re-registra el rate limiter con limites BAJOS: este IConfigureOptions
            // se registra DESPUES del de AddRateLimitingInternal y sobreescribe el
            // GlobalLimiter, produciendo el limite de 10/60s para /login.
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>(
                    new Microsoft.Extensions.Options.ConfigureNamedOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>(
                        Microsoft.Extensions.Options.Options.DefaultName,
                        options =>
                        {
                            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                            options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                            {
                                bool esAuth = httpContext.Request.Path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase)
                                           || httpContext.Request.Path.StartsWithSegments("/register", StringComparison.OrdinalIgnoreCase)
                                           || httpContext.Request.Path.StartsWithSegments("/logout", StringComparison.OrdinalIgnoreCase)
                                           || httpContext.Request.Path.StartsWithSegments("/refresh", StringComparison.OrdinalIgnoreCase);
                                return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                                    partitionKey: (esAuth ? "auth:" : "global:") + (httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"),
                                    factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                                    {
                                        PermitLimit = esAuth ? 10 : 100,
                                        Window = TimeSpan.FromSeconds(60)
                                    });
                            });
                        }));
            });
        }
    }
}
