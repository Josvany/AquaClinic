namespace AquaClinic.Api.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }

        public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestContextLoggingMiddleware>();
        }
    }
}
