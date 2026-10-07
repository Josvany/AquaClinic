using AquaClinic.Api.Authorization;
using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Api.Extensions;
using AquaClinic.Api.Infrastructure;
using AquaClinic.Api.Middleware;
using AquaClinic.Api.Realtime;
using AquaClinic.Api.Services;
using AquaClinic.Application;
using AquaClinic.Application.Contracts;
using AquaClinic.Identity;
using AquaClinic.Identity.Models;
using AquaClinic.Infrastructure;
using AquaClinic.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AquaClinic.Api
{
    public static class StartupExtensions
    {
        public static WebApplication ConfigureServices(
            this WebApplicationBuilder builder)
        {
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddIdentityServices<AquaClinicDbContext>(builder.Configuration);

            builder.Services.AddScoped<ILoggedInUserService, LoggedInUserService>();

            // ---- Agenda en tiempo real (SignalR) ----
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IReservasSuavesStore, ReservasSuavesStore>();
            builder.Services.AddSingleton<IAgregadorAgenda, AgendaNotificador>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers();

            builder.Services.AddCors(
                options => options.AddPolicy(
                    "open",
                    policy => policy.WithOrigins([builder.Configuration["ApiUrl"] ?? "https://localhost:7021",
                        builder.Configuration["BlazorUrl"] ?? "https://localhost:7081"])
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
                {
                    Title = "AquaClinic API",
                    Description = "API de gesti\u00f3n de citas para Aqua Skin and Glow Clinic (Langley, BC). " +
                                  "Cat\u00e1logo de servicios, especialidades, citas y cuenta de usuarios.",
                    Version = "v1"
                });
            });

            // ---- Medidas de seguridad ----

            // Rate limiting (global por usuario/IP + politica estricta en rutas de autenticacion)
            builder.Services.AddRateLimitingInternal(builder.Configuration);

            // Manejo global de excepciones no controladas (500 ProblemDetails)
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // Health checks de la base de datos unica
            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("AquaClinicConnectionString")!,
                    name: "AquaClinicDb",
                    tags: ["ready"]);

            // Observabilidad (OpenTelemetry; exporta solo si se define OTEL_EXPORTER_OTLP_ENDPOINT)
            builder.Services.AddObservability(builder.Configuration, "AquaClinic.Api");

            // ---- Autorizacion por permisos granulares ----
            builder.Services.AddScoped<PermissionProvider>();
            builder.Services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            // Autenticacion JWT (sin MapIdentityApi: el login es POST api/usuarios/login,
            // que emite access + refresh tokens; el logout revoca via api/usuarios/logout).

            app.UseCors("open");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // Redirige la raiz a Swagger en Development (facilita encontrar la documentacion)
                app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            }

            // CorrelationId en todos los logs de la peticion
            app.UseRequestContextLogging();

            // Fallback global: captura lo que el middleware de dominio no maneja (500 ProblemDetails).
            // Debe estar ANTES (mas afuera) para que las excepciones fluyan primero por el
            // middleware de dominio (conocidas -> 400/404) y solo las desconocidas lleguen aqui.
            app.UseExceptionHandler();

            // Excepciones conocidas del dominio (validacion, bad request, no encontrado)
            app.UseCustomExceptionHandler();

            app.UseHttpsRedirection();

            // Explicitos para que el rate limiter pueda particionar por usuario autenticado
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapControllers();

            // Hub de la agenda de reservas (SignalR; permite conexiones anonimas)
            app.MapHub<AgendaHub>("/hubs/agenda");

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }

        /// <summary>
        /// Aplica las migraciones pendientes (sin borrar la BD) y siembra
        /// roles + usuarios demo. Solo hace Migrate, NUNCA EnsureDeleted.
        /// </summary>
        public static async Task MigrateAndSeedAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetService<AquaClinicDbContext>();
                if (context != null)
                {
                    await context.Database.MigrateAsync();
                }

                await SeedRolesYAdminInicialAsync(scope.ServiceProvider, app.Environment, app.Configuration);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al migrar/sembrar la base de datos de la API (AquaClinicDbContext)");
            }
        }

        /// <summary>
        /// Asegura la existencia de los roles del sistema y, solo en Development,
        /// crea el usuario administrador demo (credenciales en AdminInicial:Email /
        /// AdminInicial:Password, con valores por defecto documentados en el README).
        /// </summary>
        private static async Task SeedRolesYAdminInicialAsync(
            IServiceProvider services,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            var roleManager = services.GetService<RoleManager<IdentityRole>>();
            var userManager = services.GetService<UserManager<ApplicationUser>>();
            if (roleManager is null || userManager is null)
            {
                return;
            }

            foreach (var rol in RolesClinica.Todos)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol) { Id = Guid.CreateVersion7().ToString() });
                }
            }

            if (!environment.IsDevelopment())
            {
                return;
            }

            var email = configuration["AdminInicial:Email"] ?? "admin@aquaclinic.com";
            var password = configuration["AdminInicial:Password"] ?? "AquaClinic#2026";

            if (await userManager.FindByEmailAsync(email) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Nombre = "Aqua",
                    Apellidos = "Aqua"
                };

                await userManager.CreateAsync(admin, password);
                await userManager.AddToRoleAsync(admin, RolesClinica.Administrador);
            }

            // Usuario del due�o del proyecto (rol Administrador)
            var emailJosvany = configuration["UsuarioJosvany:Email"] ?? "josvanyg@mail.com";
            var passwordJosvany = configuration["UsuarioJosvany:Password"] ?? "JosvanyG#2026";

            if (await userManager.FindByEmailAsync(emailJosvany) is null)
            {
                var josvany = new ApplicationUser
                {
                    UserName = emailJosvany,
                    Email = emailJosvany,
                    Nombre = "Josvany",
                    Apellidos = "Gonz�lez"
                };

                await userManager.CreateAsync(josvany, passwordJosvany);
                await userManager.AddToRoleAsync(josvany, RolesClinica.Administrador);
            }
        }
    }
}
