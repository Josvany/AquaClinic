using AutoMapper;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AquaClinic.App.Services
{
    public class CitaDataService : BaseDataService, ICitaDataService
    {
        private readonly IMapper _mapper;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public CitaDataService(IClient client, IMapper mapper) : base(client)
        {
            _mapper = mapper;
        }

        public async Task<List<CitaListViewModel>> GetAllCitas()
        {
            var allCitas = await _client.GetAllCitasAsync();
            var mappedCitas = _mapper.Map<ICollection<CitaListViewModel>>(allCitas);
            return mappedCitas.ToList();
        }

        /// <summary>
        /// Pagina del listado de citas (server-side) para el DataGrid
        /// de la agenda interna. Usa HttpClient directo (el cliente tipado
        /// NSwag no incluye el endpoint nuevo; regenerarlo exigiria la API
        /// corriendo).
        /// </summary>
        public async Task<CitasPaginadasViewModel> GetCitasPaginadas(int pagina, int tamanaPagina)
        {
            var response = await _client.HttpClient
                .GetFromJsonAsync<CitasPaginadasViewModel>(
                    $"api/citas/paginadas?pagina={pagina}&tamanaPagina={tamanaPagina}", _jsonOptions);

            return response ?? new CitasPaginadasViewModel();
        }

        /// <summary>
        /// Citas del usuario autenticado ("Mis citas"). Usa HttpClient
        /// directo por el mismo motivo.
        /// </summary>
        public async Task<List<MiCitaViewModel>> GetMisCitas()
        {
            try
            {
                var citas = await _client.HttpClient
                    .GetFromJsonAsync<List<MiCitaViewModel>>("api/citas/mias", _jsonOptions);

                return citas ?? [];
            }
            catch (HttpRequestException)
            {
                return [];
            }
        }

        /// <summary>
        /// Detalle de la cita con HttpClient directo: el cliente NSwag
        /// trae el contrato antiguo (nombre/precio/doctor/imagen) que no
        /// corresponde a la entidad Cita de la BD.
        /// </summary>
        public async Task<CitaDetalleViewModel?> ObtenerDetalleAsync(Guid id)
        {
            try
            {
                return await _client.HttpClient
                    .GetFromJsonAsync<CitaDetalleViewModel>($"api/citas/{id}", _jsonOptions);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>Mueve la cita a otro horario (PUT api/citas/{id}/mover).</summary>
        public async Task<ApiResponse<Guid>> MoverCitaAsync(Guid citaId, DateTime nuevoInicioLocal, int duracionMinutos)
        {
            try
            {
                var body = new { NuevoInicioLocal = nuevoInicioLocal, DuracionMinutos = duracionMinutos };
                var response = await _client.HttpClient
                    .PutAsJsonAsync($"api/citas/{citaId}/mover", body, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var id = await response.Content.ReadFromJsonAsync<Guid>(_jsonOptions);
                    return new ApiResponse<Guid> { Data = id, Success = true };
                }

                var mensaje = await LeerMensajeErrorAsync(response);
                return new ApiResponse<Guid> { Success = false, Message = mensaje };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse<Guid> { Success = false, Message = "No se pudo conectar con la API." };
            }
        }

        /// <summary>Cancela la cita (POST api/citas/{id}/cancelar).</summary>
        public async Task<ApiResponse<string>> CancelarCitaAsync(Guid citaId)
        {
            try
            {
                var response = await _client.HttpClient
                    .PostAsync($"api/citas/{citaId}/cancelar", null);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string> { Data = citaId.ToString(), Success = true };
                }

                var mensaje = await LeerMensajeErrorAsync(response);
                return new ApiResponse<string> { Success = false, Message = mensaje };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse<string> { Success = false, Message = "No se pudo conectar con la API." };
            }
        }

        /// <summary>
        /// La API responde errores como arreglo de validaciones (400) o
        /// { "error": "..." } (409). Se extrae el texto legible.
        /// </summary>
        private static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage response)
        {
            var cuerpo = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(cuerpo);

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var errores = doc.RootElement
                        .EnumerateArray()
                        .Select(e => e.GetString())
                        .Where(s => !string.IsNullOrWhiteSpace(s));

                    var unido = string.Join(" ", errores);
                    if (!string.IsNullOrWhiteSpace(unido))
                    {
                        return unido;
                    }
                }

                if (doc.RootElement.TryGetProperty("error", out var error)
                    && !string.IsNullOrWhiteSpace(error.GetString()))
                {
                    return error.GetString()!;
                }
            }
            catch
            {
                /* cuerpo no JSON: mensaje por defecto */
            }

            return response.StatusCode == HttpStatusCode.Conflict
                ? "Ese horario acaba de ser tomado. Elige otro disponible."
                : "No se pudo completar la operación. Intenta de nuevo.";
        }
    }
}
