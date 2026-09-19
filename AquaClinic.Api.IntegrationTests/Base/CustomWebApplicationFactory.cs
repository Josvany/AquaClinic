using AquaClinic.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Api.IntegrationTests.Base
{
    public class CustomWebApplicationFactory<TProgram>
            : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Entorno dedicado a pruebas: evita que el Program de la API reinicie la base de datos
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
        /// Prepara la base de datos de pruebas usando el proveedor de servicios
        /// REAL de la aplicacion hosteada (garantiza que se use el mismo store
        /// InMemory que usan los handlers).
        /// </summary>
        public void SeedDatabase()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AquaClinicDbContext>();
            Utilities.InitializeDbForTests(context);
        }

        public HttpClient GetAnonymousClient()
        {
            return CreateClient();
        }
    }
}
