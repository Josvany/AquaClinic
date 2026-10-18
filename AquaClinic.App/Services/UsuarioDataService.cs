using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// Consume los endpoints de /api/usuarios usando el HttpClient compartido.
    /// La cookie del navegador viaja automaticamente (same-domain localhost).
    /// </summary>
    public class UsuarioDataService : IUsuarioDataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public UsuarioDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UsuarioViewModel?> LoginAsync(string email, string password, bool emitirCookie)
        {
            var url = emitirCookie ? "api/usuarios/login?useCookies=true" : "api/usuarios/login?useCookies=false";
            var response = await _httpClient.PostAsJsonAsync(url, new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UsuarioViewModel>(_jsonOptions);
        }

        public async Task<UsuarioViewModel?> GetUsuarioActualAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UsuarioViewModel>("api/usuarios/actual", _jsonOptions);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<List<ClienteBusquedaViewModel>> BuscarClientesAsync(string? texto)
        {
            try
            {
                var url = "api/usuarios/clientes?limite=50";
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    url += $"&texto={Uri.EscapeDataString(texto.Trim())}";
                }

                var clientes = await _httpClient.GetFromJsonAsync<List<ClienteBusquedaViewModel>>(url, _jsonOptions);
                return clientes ?? [];
            }
            catch (HttpRequestException)
            {
                return [];
            }
        }

        public async Task<ApiResponse> ActualizarPerfilAsync(string? nombre, string? apellidos, string? telefono)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/usuarios/actual",
                    new { nombre, apellidos, telefono });

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }

                return await ConvertirError(response);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse { Success = false, Message = "No se pudo conectar con la API.", ValidationErrors = ex.Message };
            }
        }

        public async Task<ApiResponse> CambiarPasswordAsync(string passwordActual, string passwordNueva)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/usuarios/cambiar-password",
                    new { passwordActual, passwordNueva });

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }

                return await ConvertirError(response);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse { Success = false, Message = "No se pudo conectar con la API.", ValidationErrors = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> OlvidePasswordAsync(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/usuarios/olvide-password", new { email });

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadFromJsonAsync<OlvidePasswordResponse>(_jsonOptions);
                    return new ApiResponse<string> { Success = true, Data = body?.Token };
                }

                return await ConvertirError<string>(response);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<string> { Success = false, Message = "No se pudo conectar con la API.", ValidationErrors = ex.Message };
            }
        }

        public async Task<ApiResponse> RestablecerPasswordAsync(string email, string token, string passwordNueva)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/usuarios/restablecer-password",
                    new { email, token, passwordNueva });

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }

                return await ConvertirError(response);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse { Success = false, Message = "No se pudo conectar con la API.", ValidationErrors = ex.Message };
            }
        }

        private static async Task<ApiResponse<T>> ConvertirError<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                try
                {
                    var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("error", out var error))
                    {
                        return new ApiResponse<T> { Success = false, Message = error.GetString() ?? "" };
                    }
                    return new ApiResponse<T> { Success = false, Message = "Errores de validaci\u00f3n.", ValidationErrors = body };
                }
                catch
                {
                    return new ApiResponse<T> { Success = false, Message = "Errores de validaci\u00f3n.", ValidationErrors = body };
                }
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Sesi\u00f3n expirada. Inicia sesi\u00f3n nuevamente.",
                    HttpStatusCode.NotFound => "No se encontr\u00f3 el recurso solicitado.",
                    HttpStatusCode.TooManyRequests => "Demasiados intentos. Espera un momento.",
                    _ => "Ocurri\u00f3 un error inesperado."
                }
            };
        }

        private static async Task<ApiResponse> ConvertirError(HttpResponseMessage response)
        {
            var resultado = await ConvertirError<object>(response);
            return new ApiResponse
            {
                Success = resultado.Success,
                Message = resultado.Message,
                ValidationErrors = resultado.ValidationErrors
            };
        }

        public async Task<ApiResponse> EliminarCuentaAsync()
        {
            try
            {
                var response = await _httpClient.DeleteAsync("api/usuarios/actual");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }

                return await ConvertirError(response);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse { Success = false, Message = "No se pudo conectar con la API.", ValidationErrors = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> ExportarDatosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/usuarios/actual/exportar");

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<string> { Success = true, Data = body };
                }

                return await ConvertirError<string>(response);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<string> { Success = false, Message = "No se pudo conectar con la API.", ValidationErrors = ex.Message };
            }
        }

        private record OlvidePasswordResponse(bool SolicitudAceptada, string? Token);
    }
}
