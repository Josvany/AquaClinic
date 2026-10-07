using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// CRUD de promociones del catalogo para el personal. Consume la API via el
    /// HttpClient compartido de la App (AuthHeaderHandler inyecta el JWT y renueva
    /// la sesion automaticamente). La API exige permisos de administracion
    /// (promociones:ver/crear/editar/eliminar).
    /// La vigencia viaja en UTC y se convierte a hora local de la clinica
    /// (AgendaCalendario) para mostrarla y editarla.
    /// </summary>
    public class PromocionDataService : IPromocionDataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public PromocionDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PromocionViewModel>> ObtenerTodasAsync()
        {
            var promociones = await _httpClient.GetFromJsonAsync<List<PromocionApiDto>>("api/promociones", _jsonOptions);

            return promociones?
                .Select(p => new PromocionViewModel
                {
                    PromocionId = p.PromocionId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    TipoDescuento = (int)p.TipoDescuento,
                    Valor = p.Valor,
                    VigenciaDesde = p.VigenciaDesde is DateTimeOffset desde ? AgendaCalendario.ALocal(desde) : null,
                    VigenciaHasta = p.VigenciaHasta is DateTimeOffset hasta ? AgendaCalendario.ALocal(hasta) : null,
                    Activo = p.Activo,
                    ServicioIds = p.ServicioIds ?? []
                })
                .ToList() ?? [];
        }

        public async Task<ApiResponse<Guid>> CrearAsync(PromocionViewModel promocion)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/promociones", ToCommand(promocion), _jsonOptions);

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

        public async Task<ApiResponse> ActualizarAsync(PromocionViewModel promocion)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/promociones", ToCommand(promocion), _jsonOptions);

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
                var response = await _httpClient.DeleteAsync($"api/promociones/{id}");

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

        private static object ToCommand(PromocionViewModel p) => new
        {
            promocionId = p.PromocionId,
            nombre = p.Nombre,
            descripcion = p.Descripcion,
            tipoDescuento = p.TipoDescuento,
            valor = p.Valor,
            vigenciaDesde = p.VigenciaDesde is DateTime desde ? (DateTimeOffset?)AgendaCalendario.AUtc(desde) : null,
            vigenciaHasta = p.VigenciaHasta is DateTime hasta ? (DateTimeOffset?)AgendaCalendario.AUtc(hasta) : null,
            activo = p.Activo,
            servicioIds = p.ServicioIds
        };

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
                    // ValidationException: ["mensaje", ...]
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

        private sealed record PromocionApiDto(
            Guid PromocionId,
            string Nombre,
            string? Descripcion,
            int TipoDescuento,
            decimal Valor,
            DateTimeOffset? VigenciaDesde,
            DateTimeOffset? VigenciaHasta,
            bool Activo,
            List<Guid>? ServicioIds);
    }
}