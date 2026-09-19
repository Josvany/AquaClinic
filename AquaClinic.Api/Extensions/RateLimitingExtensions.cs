using System.Threading.RateLimiting;
using AquaClinic.Api.Infrastructure;

namespace AquaClinic.Api.Extensions
{
    internal static class RateLimitingExtensions
    {
        // Rutas de autenticaci&#243;n de MapIdentityApi (y logout propio) que reciben un
        // l&#237;mite m&#225;s estricto para frenar intentos de fuerza bruta.
        private static readonly string[] RutasDeAutenticacion =
        [
            "/login",
            "/register",
            "/logout",
            "/refresh"
        ];

        internal static IServiceCollection AddRateLimitingInternal(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            int globalPermitLimit = configuration.GetValue<int?>("RateLimiting:Global:PermitLimit") ?? 100;
            int globalWindowSeconds = configuration.GetValue<int?>("RateLimiting:Global:WindowInSeconds") ?? 60;
            int authPermitLimit = configuration.GetValue<int?>("RateLimiting:Authentication:PermitLimit") ?? 10;
            int authWindowSeconds = configuration.GetValue<int?>("RateLimiting:Authentication:WindowInSeconds") ?? 60;

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Limite global de ventana fija, particionado por usuario autenticado o IP del cliente.
                // Las rutas de autenticaci&#243;n reciben la ventana estricta (adaptaci&#243;n necesaria porque
                // los endpoints de MapIdentityApi no admiten RequireRateLimiting).
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    // El prefijo en la clave es necesario: PartitionedRateLimiter cachea el
                    // limiter por clave, de modo que la particion estricta de autenticacion
                    // y la global de un mismo usuario/IP deben ser particiones distintas.
                    bool esRutaDeAutenticacion = RutasDeAutenticacion
                        .Any(ruta => httpContext.Request.Path.StartsWithSegments(ruta, StringComparison.OrdinalIgnoreCase));

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: (esRutaDeAutenticacion ? "auth:" : "global:") + GetPartitionKey(httpContext),
                        factory: _ => esRutaDeAutenticacion
                            ? new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = authPermitLimit,
                                Window = TimeSpan.FromSeconds(authWindowSeconds)
                            }
                            : new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = globalPermitLimit,
                                Window = TimeSpan.FromSeconds(globalWindowSeconds)
                            });
                });

                // Pol&#237;tica nombrada para decorar endpoints propios (por ejemplo, un futuro AuthController JWT).
                options.AddPolicy(RateLimitingPolicies.Authentication, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetPartitionKey(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = authPermitLimit,
                            Window = TimeSpan.FromSeconds(authWindowSeconds)
                        }));
            });

            return services;
        }

        private static string GetPartitionKey(HttpContext httpContext)
        {
            return httpContext.User.Identity?.Name
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";
        }
    }
}
