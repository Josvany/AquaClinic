using System.Net;
using System.Text.Json;

namespace AquaClinic.App.Services.Base
{
    /// <summary>
    /// Helper centralizado para convertir respuestas de error de la API en objetos ApiResponse.
    /// Maneja arreglos de mensajes de validación (ValidationException), objetos de error { error: "..." },
    /// respuestas RFC ProblemDetails { detail: "...", title: "..." } y códigos de estado HTTP (403, 404, etc.).
    /// </summary>
    public static class ApiResponseHelper
    {
        public static async Task<ApiResponse<T>> MapearErrorAsync<T>(HttpResponseMessage response)
        {
            var api = new ApiResponse<T> { Success = false };

            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    using var doc = JsonDocument.Parse(content);

                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var mensaje in doc.RootElement.EnumerateArray())
                        {
                            api.ValidationErrors += mensaje.GetString() + Environment.NewLine;
                        }
                    }
                    else if (doc.RootElement.TryGetProperty("errors", out var errores))
                    {
                        foreach (var propiedad in errores.EnumerateObject())
                        {
                            foreach (var mensaje in propiedad.Value.EnumerateArray())
                            {
                                api.ValidationErrors += mensaje.GetString() + Environment.NewLine;
                            }
                        }
                    }
                    else if (doc.RootElement.TryGetProperty("error", out var error))
                    {
                        api.Message = error.GetString() ?? string.Empty;
                    }
                    else if (doc.RootElement.TryGetProperty("detail", out var detail))
                    {
                        api.Message = detail.GetString() ?? string.Empty;
                    }
                    else if (doc.RootElement.TryGetProperty("title", out var title))
                    {
                        api.Message = title.GetString() ?? string.Empty;
                    }
                }
            }
            catch (JsonException)
            {
                // Cuerpo vacio o no JSON (p. ej. un 404 sin cuerpo): mensaje generico
            }

            if (response.StatusCode == HttpStatusCode.Forbidden && string.IsNullOrEmpty(api.Message))
            {
                api.Message = "No tienes permisos para esta acción.";
            }

            if (string.IsNullOrEmpty(api.Message) && string.IsNullOrEmpty(api.ValidationErrors))
            {
                api.Message = $"Ocurrió un error ({(int)response.StatusCode}). Intenta nuevamente.";
            }

            return api;
        }

        public static async Task<ApiResponse> MapearErrorAsync(HttpResponseMessage response)
        {
            var conDatos = await MapearErrorAsync<object>(response);
            return new ApiResponse
            {
                Message = conDatos.Message,
                ValidationErrors = conDatos.ValidationErrors,
                Success = false
            };
        }
    }
}
