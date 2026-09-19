using AquaClinic.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace AquaClinic.Api.Middleware
{
    /// <summary>
    /// Maneja las excepciones conocidas del dominio (validaci&#243;n, bad request, no encontrado).
    /// Las excepciones no controladas se relanzan y las captura el
    /// <see cref="Infrastructure.GlobalExceptionHandler"/>, que devuelve un ProblemDetails
    /// gen&#233;rico sin exponer detalles internos.
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException validationException)
            {
                await HandleValidationExceptionAsync(context, validationException);
            }
            catch (BadRequestException badRequestException)
            {
                await HandleBadRequestExceptionAsync(context, badRequestException);
            }
            catch (NotFoundException)
            {
                await HandleNotFoundExceptionAsync(context);
            }
            // Las dem&#225;s excepciones no se capturan aqui: suben al IExceptionHandler
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
