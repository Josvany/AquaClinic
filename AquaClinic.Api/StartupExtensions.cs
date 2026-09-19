using AquaClinic.Api.Extensions;
using AquaClinic.Api.Infrastructure;
using AquaClinic.Api.Middleware;
using AquaClinic.Api.Services;
using AquaClinic.Application;
using AquaClinic.Application.Contracts;
using AquaClinic.Identity;
using AquaClinic.Identity.Models;
using AquaClinic.Infrastructure;
using AquaClinic.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

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
            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddScoped<ILoggedInUserService, LoggedInUserService>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers();

            builder.Services.AddCors(
                options => options.AddPolicy(
                    "open",
                    policy => policy.WithOrigins([builder.Configuration["ApiUrl"] ?? "https://localhost:7081",
                        builder.Configuration["BlazorUrl"] ?? "https://localhost:7080"])
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ---- Medidas de seguridad ----

            // Rate limiting (global por usuario/IP + politica estricta en rutas de autenticacion)
            builder.Services.AddRateLimitingInternal(builder.Configuration);

            // Manejo global de excepciones no controladas (500 ProblemDetails)
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // Health checks de las dos bases de datos
            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("AquaClinicConnectionString")!,
                    name: "AquaClinicDb",
                    tags: ["ready"])
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("AquaClinicIdentityConnectionString")!,
                    name: "AquaClinicIdentityDb",
                    tags: ["ready"]);

            // Observabilidad (OpenTelemetry; exporta solo si se define OTEL_EXPORTER_OTLP_ENDPOINT)
            builder.Services.AddObservability(builder.Configuration, "AquaClinic.Api");

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.MapIdentityApi<ApplicationUser>();

            app.MapPost("/Logout", async (ClaimsPrincipal user, SignInManager<ApplicationUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.Ok();
            });

            app.UseCors("open");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // CorrelationId en todos los logs de la peticion
            app.UseRequestContextLogging();

            // Excepciones conocidas del dominio + fallback global (500 ProblemDetails)
            app.UseCustomExceptionHandler();
            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            // Explicitos para que el rate limiter pueda particionar por usuario autenticado
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapControllers();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }

        public static async Task ResetDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetService<AquaClinicDbContext>();
                if (context != null)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al reiniciar la base de datos de la API (AquaClinicDbContext)");
            }
        }
    }
}
