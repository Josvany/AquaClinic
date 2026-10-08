using AquaClinic.Api.IntegrationTests.Base;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using AquaClinic.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AquaClinic.Api.IntegrationTests.Controllers
{
    /// <summary>
    /// Verifica la reprogramacion de citas propias (PUT api/citas/mias/{id}/mover):
    /// el cliente solo puede mover citas que le pertenecen y solo si la fecha de
    /// atencion todavia no paso. El handler de pruebas autentica con rol
    /// Administrador y permite fijar el usuario con X-Test-UserId.
    /// </summary>
    [Collection("ApiIntegration")]
    public class MoverMisCitasControllerTests
    {
        private const string UsuarioDueño = "0195a1b2-c3d4-7000-8000-00000000000a";
        private const string UsuarioAjeno = "0195a1b2-c3d4-7000-8000-00000000000b";

        private readonly CustomWebApplicationFactory<Program> _factory;

        public MoverMisCitasControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.SeedDatabase();
        }

        /// <summary>
        /// Crea una cita en el futuro y devuelve su id. Si se indica otro
        /// UsuarioId, la cita queda a nombre de ese usuario.
        /// </summary>
        private Guid CrearCitaPropia(string usuarioId, DateTimeOffset inicioUtc)
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AquaClinicDbContext>();

            // Servicio del seed (Hydrafacial, 60 min).
            var servicio = context.Servicios.FirstOrDefault()
                ?? throw new InvalidOperationException("El seed no tiene servicios.");

            var cita = new Cita
            {
                UsuarioId = usuarioId,
                FechaInicio = inicioUtc,
                FechaFin = inicioUtc.AddMinutes(60),
                Estado = EstadoCita.Pendiente
            };
            cita.CitasServicios = new List<CitaServicio>
            {
                new() { Cita = cita, Servicio = servicio }
            };

            context.Citas.Add(cita);
            context.SaveChanges();

            return cita.CitaId;
        }

        /// <summary>
        /// Proximo dia laborable (lunes a sabado) a las 10:00 en hora local de
        /// la clinica, respetando el maximo de 60 dias de anticipacion.
        /// </summary>
        private static DateTime ProximoDiaLaborable()
        {
            var hoy = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver")).Date;

            for (var i = 1; i <= 60; i++)
            {
                var dia = hoy.AddDays(i);
                if (dia.DayOfWeek != DayOfWeek.Sunday)
                {
                    return dia.AddHours(10);
                }
            }

            return hoy.AddDays(1).AddHours(10);
        }

        /// <summary>
        /// Cliente que se autentica como un usuario concreto con rol
        /// Cliente: es quien tiene el permiso miscitas:gestionar.
        /// </summary>
        private HttpClient ClienteComo(string usuarioId)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-UserId", usuarioId);
            client.DefaultRequestHeaders.Add("X-Test-Rol", "Cliente");
            return client;
        }

        [Fact]
        public async Task MoverMisCita_DeOtroUsuario_Retorna404()
        {
            // La cita es de UsuarioDueño, pero se intenta mover como UsuarioAjeno.
            var citaId = CrearCitaPropia(UsuarioDueño, DateTimeOffset.UtcNow.AddDays(7));
            var client = ClienteComo(UsuarioAjeno);

            var response = await client.PutAsJsonAsync($"/api/citas/mias/{citaId}/mover",
                new { NuevoInicioLocal = ProximoDiaLaborable(), DuracionMinutos = 60 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task MoverMisCita_PropiaYMover_FechaActualizada()
        {
            var citaId = CrearCitaPropia(UsuarioDueño, DateTimeOffset.UtcNow.AddDays(3));
            var client = ClienteComo(UsuarioDueño);

            var nuevoInicioLocal = ProximoDiaLaborable();

            var response = await client.PutAsJsonAsync($"/api/citas/mias/{citaId}/mover",
                new { NuevoInicioLocal = nuevoInicioLocal, DuracionMinutos = 60 });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AquaClinicDbContext>();
            var cita = await context.Citas.AsNoTracking()
                .FirstAsync(c => c.CitaId == citaId);

            // La cita quedo en el nuevo horario (en hora local de la clinica,
            // respetando el offset real de la fecha, no el horario fijo).
            var zona = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
            var inicioLocal = TimeZoneInfo.ConvertTime(cita.FechaInicio, zona).DateTime;

            Assert.Equal(nuevoInicioLocal.Date, inicioLocal.Date);
            Assert.Equal(nuevoInicioLocal.Hour, inicioLocal.Hour);
        }

        [Fact]
        public async Task MoverMisCita_QueYaPaso_Retorna409()
        {
            // Cita cuya fecha de atencion ya paso: no se puede reprogramar.
            var citaId = CrearCitaPropia(UsuarioDueño, DateTimeOffset.UtcNow.AddDays(-2));
            var client = ClienteComo(UsuarioDueño);

            var response = await client.PutAsJsonAsync($"/api/citas/mias/{citaId}/mover",
                new { NuevoInicioLocal = ProximoDiaLaborable(), DuracionMinutos = 60 });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task MoverMisCita_Cancelada_Retorna409()
        {
            var citaId = CrearCitaPropia(UsuarioDueño, DateTimeOffset.UtcNow.AddDays(4));

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AquaClinicDbContext>();
                var cita = await context.Citas.FirstAsync(c => c.CitaId == citaId);
                cita.Estado = EstadoCita.Cancelada;
                await context.SaveChangesAsync();
            }

            var client = ClienteComo(UsuarioDueño);

            var response = await client.PutAsJsonAsync($"/api/citas/mias/{citaId}/mover",
                new { NuevoInicioLocal = ProximoDiaLaborable(), DuracionMinutos = 60 });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task CancelarMisCita_Propia_PasaACancelada()
        {
            var citaId = CrearCitaPropia(UsuarioDueño, DateTimeOffset.UtcNow.AddDays(6));
            var client = ClienteComo(UsuarioDueño);

            var response = await client.PostAsync($"/api/citas/mias/{citaId}/cancelar", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AquaClinicDbContext>();
            var cita = await context.Citas.AsNoTracking()
                .FirstAsync(c => c.CitaId == citaId);

            // Estado 3 = Cancelada.
            Assert.Equal(3, (int)cita.Estado);
        }

        [Fact]
        public async Task CancelarMisCita_DeOtroUsuario_Retorna404()
        {
            var citaId = CrearCitaPropia(UsuarioDueño, DateTimeOffset.UtcNow.AddDays(6));
            var client = ClienteComo(UsuarioAjeno);

            var response = await client.PostAsync($"/api/citas/mias/{citaId}/cancelar", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CancelarMisCita_QueYaPaso_Retorna409()
        {
            var citaId = CrearCitaPropia(UsuarioDueño, DateTimeOffset.UtcNow.AddDays(-2));
            var client = ClienteComo(UsuarioDueño);

            var response = await client.PostAsync($"/api/citas/mias/{citaId}/cancelar", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task GetMisCitas_IncluyeEstadoValorYDuracion()
        {
            var inicio = DateTimeOffset.UtcNow.AddDays(5);
            var citaId = CrearCitaPropia(UsuarioDueño, inicio);
            var client = ClienteComo(UsuarioDueño);

            var response = await client.GetAsync("/api/citas/mias");
            response.EnsureSuccessStatusCode();

            var citas = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

            Assert.NotNull(citas);

            var cita = Assert.Single(citas.Where(c =>
                c.GetProperty("citaId").GetGuid() == citaId));

            // EstadoValor 1 = Pendiente; el cliente lo usa para decidir si admite cambio.
            Assert.Equal(1, cita.GetProperty("estadoValor").GetInt32());
            Assert.True(cita.GetProperty("duracionMinutos").GetInt32() > 0);
        }
    }
}