using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// CRUD de bloqueos de la clinica para el personal. Consume la API via
    /// HttpClient compartido de la App (AuthHeaderHandler inyecta el JWT y
    /// renueva la sesion automaticamente). La API exige permisos de
    /// administracion (bloqueos:ver/crear/editar/eliminar).
    /// Las fechas viajan en UTC y se convierten a hora local de la clinica
    /// (AgendaCalendario) para mostrarlas y editarlas.
    /// </summary>
    public class BloqueoDataService : IBloqueoDataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public BloqueoDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<BloqueoViewModel>> ObtenerTodosAsync()
        {
            var bloqueos = await _httpClient.GetFromJsonAsync<List<BloqueoApiDto>>("api/bloqueos", _jsonOptions);

            return bloqueos?
                .Select(b => new BloqueoViewModel
                {
                    BloqueoId = b.BloqueoId,
                    Motivo = b.Motivo,
                    Inicio = AgendaCalendario.ALocal(b.FechaInicio),
                    Fin = AgendaCalendario.ALocal(b.FechaFin)
                })
                .ToList() ?? [];
        }

        public async Task<ApiResponse<Guid>> CrearAsync(BloqueoViewModel bloqueo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/bloqueos", ToCommand(bloqueo), _jsonOptions);

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

        public async Task<ApiResponse> ActualizarAsync(BloqueoViewModel bloqueo)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/bloqueos", ToCommand(bloqueo), _jsonOptions);

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
                var response = await _httpClient.DeleteAsync($"api/bloqueos/{id}");

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

        private static object ToCommand(BloqueoViewModel b) => new
        {
            bloqueoId = b.BloqueoId,
            motivo = b.Motivo,
            fechaInicio = AgendaCalendario.AUtc(b.Inicio),
            fechaFin = AgendaCalendario.AUtc(b.Fin)
        };

        private static Task<ApiResponse<T>> MapearErrorAsync<T>(HttpResponseMessage response) =>
            ApiResponseHelper.MapearErrorAsync<T>(response);

        private static Task<ApiResponse> MapearErrorAsync(HttpResponseMessage response) =>
            ApiResponseHelper.MapearErrorAsync(response);

        // ---------- DTOs de la API (JsonSerializerDefaults.Web: camelCase) ----------

        private sealed record BloqueoApiDto(
            Guid BloqueoId,
            string Motivo,
            DateTimeOffset FechaInicio,
            DateTimeOffset FechaFin);
    }
}
