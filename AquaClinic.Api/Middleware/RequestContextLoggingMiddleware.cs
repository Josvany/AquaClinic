using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace AquaClinic.Api.Middleware
{
    /// <summary>
    /// Propaga el identificador de correlacion de la peticion al LogContext de Serilog,
    /// de modo que todos los logs de una misma peticion compartan el CorrelationId.
    /// </summary>
    public class RequestContextLoggingMiddleware(RequestDelegate next)
    {
        private const string CorrelationIdHeaderName = "Correlation-Id";

        public Task Invoke(HttpContext context)
        {
            using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
            {
                return next.Invoke(context);
            }
        }

        private static string GetCorrelationId(HttpContext context)
        {
            context.Request.Headers.TryGetValue(
                CorrelationIdHeaderName,
                out StringValues correlationId);

            return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
        }
    }
}
