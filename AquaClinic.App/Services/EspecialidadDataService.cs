using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// CRUD de especialidades del catalogo. Consume la API por el HttpClient
    /// compartido de la App (AuthHeaderHandler inyecta el JWT y renueva la sesion
    /// solo). No usa el cliente generado por NSwag porque esta desactualizado:
    /// pedia una ruta que ya no existe y devolvia 404 con cuerpo vacio, lo que
    /// reventaba la pagina con un error de deserializacion de JSON.
    /// La API exige permisos de administracion
    /// (especialidades:ver/crear/editar/eliminar).
    /// </summary>
    public class EspecialidadDataService : IEspecialidadDataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public EspecialidadDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EspecialidadViewModel>> ObtenerTodasAsync()
        {
            // allwithservicios devuelve especialidad + sus servicios
            var especialidades = await _httpClient
                .GetFromJsonAsync<List<EspecialidadApiDto>>("api/especialidades/allwithservicios", _jsonOptions);

            return especialidades?
                .Select(e => new EspecialidadViewModel
                {
                    EspecialidadId = e.EspecialidadId,
                    Nombre = e.Nombre,
                    CantidadServicios = e.Servicios?.Count ?? 0
                })
                .ToList() ?? [];
        }

        public async Task<ApiResponse<Guid>> CrearAsync(EspecialidadViewModel especialidad)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/especialidades",
                    new { nombre = especialidad.Nombre }, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    return await MapearErrorAsync<Guid>(response);
                }

                // La API responde 200 con { success, message, validationErrors, especialidad }
                var cuerpo = await response.Content.ReadFromJsonAsync<RespuestaEspecialidad>(_jsonOptions);

                if (cuerpo?.Success != true)
                {
                    return new ApiResponse<Guid>
                    {
                        Success = false,
                        Message = cuerpo?.Message,
                        ValidationErrors = cuerpo?.ValidationErrors is null
                            ? null
                            : string.Join(Environment.NewLine, cuerpo.ValidationErrors)
                    };
                }

                return new ApiResponse<Guid> { Success = true, Data = cuerpo.Especialidad!.EspecialidadId };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse<Guid>
                {
                    Message = "No se pudo conectar con la API. Verifica tu conexión.",
                    Success = false
                };
            }
            catch (JsonException)
            {
                // Cuerpo que no es el JSON esperado: no debe tumbar la pagina
                return new ApiResponse<Guid>
                {
                    Message = "La API devolvió una respuesta inesperada.",
                    Success = false
                };
            }
        }

        public async Task<ApiResponse> ActualizarAsync(EspecialidadViewModel especialidad)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/especialidades",
                    new { especialidadId = especialidad.EspecialidadId, nombre = especialidad.Nombre },
                    _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    return await MapearErrorAsync(response);
                }

                return new ApiResponse { Success = true };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse
                {
                    Message = "No se pudo conectar con la API. Verifica tu conexión.",
                    Success = false
                };
            }
            catch (JsonException)
            {
                return new ApiResponse
                {
                    Message = "La API devolvió una respuesta inesperada.",
                    Success = false
                };
            }
        }

        public async Task<ApiResponse> EliminarAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/especialidades/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }

                return await MapearErrorAsync(response);
            }
            catch (HttpRequestException)
            {
                return new ApiResponse
                {
                    Message = "No se pudo conectar con la API. Verifica tu conexión.",
                    Success = false
                };
            }
        }

        // ---------- Helpers ----------

        /// <summary>
        /// Convierte las respuestas de error de la API en ApiResponse: el
        /// ExceptionHandlerMiddleware responde un array de mensajes
        /// (ValidationException) o { error: "..." } (BadRequestException); el
        /// GlobalExceptionHandler responde ProblemDetails.
        /// </summary>
        private static async Task<ApiResponse<T>> MapearErrorAsync<T>(HttpResponseMessage response)
        {
            var api = new ApiResponse<T> { Success = false };

            try
            {
                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var mensaje in doc.RootElement.EnumerateArray())
                    {
                        api.ValidationErrors += mensaje.GetString() + Environment.NewLine;
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

        private static async Task<ApiResponse> MapearErrorAsync(HttpResponseMessage response)
        {
            var conDatos = await MapearErrorAsync<object>(response);
            return new ApiResponse
            {
                Message = conDatos.Message,
                ValidationErrors = conDatos.ValidationErrors,
                Success = false
            };
        }

        // ---------- DTOs de la API (JsonSerializerDefaults.Web: camelCase) ----------

        private sealed record EspecialidadApiDto(
            Guid EspecialidadId,
            string Nombre,
            List<ServicioApiDto>? Servicios);

        private sealed record ServicioApiDto(Guid ServicioId, string Nombre);

        private sealed record RespuestaEspecialidad(
            bool Success,
            string? Message,
            List<string>? ValidationErrors,
            EspecialidadDto? Especialidad);

        private sealed record EspecialidadDto(Guid EspecialidadId, string Nombre);
    }
}