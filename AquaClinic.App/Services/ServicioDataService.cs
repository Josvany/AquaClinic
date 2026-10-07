using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// CRUD de servicios del catalogo para el personal. Consume la API via
    /// HttpClient compartido de la App (AuthHeaderHandler inyecta el JWT y
    /// renueva la sesion automaticamente). La API exige permisos de
    /// administrador para escribir (servicios:crear/editar/eliminar) y responde
    /// 409 cuando un servicio con citas no se puede eliminar.
    /// </summary>
    public class ServicioDataService : IServicioDataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public ServicioDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ServicioViewModel>> ObtenerTodosAsync()
        {
            // GET api/servicios devuelve los activos con el nombre de su especialidad
            var servicios = await _httpClient.GetFromJsonAsync<List<ServicioApiDto>>("api/servicios", _jsonOptions);

            return servicios?
                .Select(s => new ServicioViewModel
                {
                    ServicioId = s.ServicioId,
                    Nombre = s.Nombre,
                    Descripcion = s.Descripcion,
                    Precio = s.Precio,
                    DuracionMinutos = s.DuracionMinutos,
                    ImagenUrl = s.ImagenUrl,
                    Activo = true,
                    EspecialidadNombre = s.EspecialidadNombre
                })
                .ToList() ?? [];
        }

        public async Task<List<ServicioViewModel>> ObtenerTodosIncluyendoInactivosAsync()
        {
            // GET api/servicios/todos trae tambien los dados de baja (Activo = false)
            var servicios = await _httpClient.GetFromJsonAsync<List<ServicioApiDto>>("api/servicios/todos", _jsonOptions);

            return servicios?
                .Select(s => new ServicioViewModel
                {
                    ServicioId = s.ServicioId,
                    Nombre = s.Nombre,
                    Descripcion = s.Descripcion,
                    Precio = s.Precio,
                    DuracionMinutos = s.DuracionMinutos,
                    ImagenUrl = s.ImagenUrl,
                    Activo = s.Activo,
                    EspecialidadNombre = s.EspecialidadNombre
                })
                .ToList() ?? [];
        }

        public async Task<ServicioViewModel?> ObtenerPorIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/servicios/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<ServicioDetalleDto>(_jsonOptions);

            if (dto is null)
            {
                return null;
            }

            return new ServicioViewModel
            {
                ServicioId = dto.ServicioId,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                DuracionMinutos = dto.DuracionMinutos,
                ImagenUrl = dto.ImagenUrl,
                EspecialidadId = dto.EspecialidadId,
                EspecialidadNombre = dto.Especialidad?.Nombre
            };
        }

        public async Task<ApiResponse<Guid>> CrearAsync(ServicioViewModel servicio)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/servicios", ToCommand(servicio), _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var id = await response.Content.ReadFromJsonAsync<Guid>(_jsonOptions);
                    return new ApiResponse<Guid> { Data = id, Success = true };
                }

                return await MapearErrorAsync<Guid>(response);
            }
            catch (HttpRequestException)
            {
                return new ApiResponse<Guid> { Message = "No se pudo conectar con la API. Verifica tu conexión.", Success = false };
            }
        }

        public async Task<ApiResponse> ActualizarAsync(ServicioViewModel servicio)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/servicios", ToCommand(servicio), _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }

                return await MapearErrorAsync(response);
            }
            catch (HttpRequestException)
            {
                return new ApiResponse { Message = "No se pudo conectar con la API. Verifica tu conexión.", Success = false };
            }
        }

        public async Task<ApiResponse> EliminarAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/servicios/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }

                return await MapearErrorAsync(response);
            }
            catch (HttpRequestException)
            {
                return new ApiResponse { Message = "No se pudo conectar con la API. Verifica tu conexión.", Success = false };
            }
        }

        // ---------- Helpers ----------

        private static object ToCommand(ServicioViewModel s) => new
        {
            servicioId = s.ServicioId,
            nombre = s.Nombre,
            precio = s.Precio,
            duracionMinutos = s.DuracionMinutos,
            descripcion = s.Descripcion,
            imagenUrl = s.ImagenUrl,
            especialidadId = s.EspecialidadId
        };

        /// <summary>Convierte ProblemDetails/ValidationProblemDetails en ApiResponse.</summary>
        private static async Task<ApiResponse<T>> MapearErrorAsync<T>(HttpResponseMessage response)
        {
            var api = new ApiResponse<T> { Success = false };

            try
            {
                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                if (doc.RootElement.TryGetProperty("errors", out var errores))
                {
                    // ValidationProblemDetails: { errors: { campo: [msg, ...] } }
                    foreach (var propiedad in errores.EnumerateObject())
                    {
                        foreach (var mensaje in propiedad.Value.EnumerateArray())
                        {
                            api.ValidationErrors += mensaje.GetString() + Environment.NewLine;
                        }
                    }
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
            catch
            {
                // Cuerpo no JSON: mensaje generico por codigo de estado
            }

            if (response.StatusCode == HttpStatusCode.Forbidden && string.IsNullOrEmpty(api.Message))
            {
                api.Message = "No tienes permisos para esta acción.";
            }

            if (string.IsNullOrEmpty(api.Message) && string.IsNullOrEmpty(api.ValidationErrors))
            {
                api.Message = "Ocurrió un error. Intenta nuevamente.";
            }

            return api;
        }

        private static async Task<ApiResponse> MapearErrorAsync(HttpResponseMessage response)
        {
            var conDatos = await MapearErrorAsync<object>(response);
            return new ApiResponse { Message = conDatos.Message, ValidationErrors = conDatos.ValidationErrors, Success = false };
        }

        // ---------- DTOs de la API (JsonSerializerDefaults.Web: camelCase) ----------

        private sealed record ServicioApiDto(
            Guid ServicioId,
            string Nombre,
            string? Descripcion,
            decimal Precio,
            int DuracionMinutos,
            string? ImagenUrl,
            string? EspecialidadNombre,
            bool Activo = true);

        private sealed record ServicioDetalleDto(
            Guid ServicioId,
            string Nombre,
            decimal Precio,
            int DuracionMinutos,
            string? Descripcion,
            string? ImagenUrl,
            Guid EspecialidadId,
            EspecialidadNombreDto? Especialidad);

        private sealed record EspecialidadNombreDto(string? Nombre);
    }
}
