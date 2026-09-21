using AquaClinic.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace AquaClinic.Api.Middleware
{
    /// <summary>
    /// Maneja las excepciones conocidas del dominio (validación, bad request, no encontrado).
    /// Las excepciones no controladas se relanzan y las captura el
    /// <see cref="Infrastructure.GlobalExceptionHandler"/> (middleware externo).
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException validationException)
            {
                _logger.LogDebug("ExceptionHandlerMiddleware: ValidationException capturada");
                await HandleValidationExceptionAsync(context, validationException);
            }
            catch (BadRequestException badRequestException)
            {
                _logger.LogDebug("ExceptionHandlerMiddleware: BadRequestException capturada - {Mensaje}", badRequestException.Message);
                await HandleBadRequestExceptionAsync(context, badRequestException);
            }
            catch (NotFoundException notFoundException)
            {
                _logger.LogDebug("ExceptionHandlerMiddleware: NotFoundException capturada - {Mensaje}", notFoundException.Message);
                await HandleNotFoundExceptionAsync(context);
            }
            // Las demás excepciones no se capturan aquí: suben al IExceptionHandler
            // registrado (GlobalExceptionHandler) que responde 500 ProblemDetails.
        }

        private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException validationException)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var result = JsonSerializer.Serialize(validationException.ValdationErrors);
            return context.Response.WriteAsync(result);
        }

        private static Task HandleBadRequestExceptionAsync(HttpContext context, BadRequestException badRequestException)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var result = JsonSerializer.Serialize(new { error = badRequestException.Message });
            return context.Response.WriteAsync(result);
        }

        private static Task HandleNotFoundExceptionAsync(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return Task.CompletedTask;
        }
    }
}
