using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// Consume el catalogo publico (GET api/servicios) y la creacion de citas
    /// (POST api/citas) con HttpClient compartido de la App.
    /// </summary>
    public class ReservasDataService : IServiciosPublicosDataService, IReservasDataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public ReservasDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ServicioPublicoVm>> ObtenerServiciosAsync()
        {
            try
            {
                // GET api/servicios es publico y devuelve solo servicios activos del catalogo
                var servicios = await _httpClient.GetFromJsonAsync<List<ServicioApiDto>>("api/servicios", _jsonOptions);
                return servicios?
                    .Select(s => new ServicioPublicoVm
                    {
                        ServicioId = s.ServicioId,
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        Precio = s.Precio,
                        DuracionMinutos = s.DuracionMinutos,
                        EspecialidadNombre = s.EspecialidadNombre,
                        NombrePromocion = s.NombrePromocion,
                        TipoDescuentoPromocion = s.TipoDescuentoPromocion,
                        ValorPromocion = s.ValorPromocion
                    }) ?? [];
            }
            catch (HttpRequestException)
            {
                return [];
            }
        }

        public async Task<ApiResponse<Guid>> CrearCitaAsync(ReservaCitaVm reserva)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/citas", reserva, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var id = await response.Content.ReadFromJsonAsync<Guid>(_jsonOptions);
                    return new ApiResponse<Guid> { Data = id, Success = true };
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var mensaje = "Ese horario acaba de ser tomado. Elige otro disponible.";

                    try
                    {
                        var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("error", out var error) && !string.IsNullOrWhiteSpace(error.GetString()))
                        {
                            mensaje = error.GetString()!;
                        }
                    }
                    catch { /* cuerpo no JSON: mensaje por defecto */ }

                    return new ApiResponse<Guid> { Success = false, Message = mensaje };
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            var errores = doc.RootElement.EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrWhiteSpace(s));
                            return new ApiResponse<Guid> { Success = false, Message = string.Join(" ", errores) };
                        }
                        if (doc.RootElement.TryGetProperty("error", out var err))
                        {
                            return new ApiResponse<Guid> { Success = false, Message = err.GetString() };
                        }
                    }
                    catch { }
                }

                return new ApiResponse<Guid> { Success = false, Message = "No se pudo crear la cita. Intenta de nuevo." };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse<Guid> { Success = false, Message = "No se pudo conectar con la API." };
            }
        }

        private sealed class ServicioApiDto
        {
            public Guid ServicioId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }
            public int DuracionMinutos { get; set; }
            public string? ImagenUrl { get; set; }
            public string? EspecialidadNombre { get; set; }
            public string? NombrePromocion { get; set; }
            public int? TipoDescuentoPromocion { get; set; }
            public decimal? ValorPromocion { get; set; }
        }
    }
}
