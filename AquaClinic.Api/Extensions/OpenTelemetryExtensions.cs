using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AquaClinic.Api.Extensions
{
    internal static class OpenTelemetryExtensions
    {
        internal static IServiceCollection AddObservability(
            this IServiceCollection services,
            IConfiguration configuration,
            string serviceName)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation())
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation());

            // Exporta a cualquier backend compatible con OTLP (Aspire, Jaeger, Grafana...)
            // solo si se configura la variable est&#225;ndar OTEL_EXPORTER_OTLP_ENDPOINT.
            if (!string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            {
                services.AddOpenTelemetry().UseOtlpExporter();
            }

            return services;
        }
    }
}
