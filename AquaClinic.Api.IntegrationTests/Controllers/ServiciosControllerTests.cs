using AquaClinic.Api.IntegrationTests.Base;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AquaClinic.Api.IntegrationTests.Controllers
{
    /// <summary>
    /// Verifica el cat&#225;logo de servicios: lectura p&#250;blica y escritura protegida
    /// por permisos (el handler de pruebas autentica con rol Administrador).
    /// </summary>
[Collection("ApiIntegration")]
    public class ServiciosControllerTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ServiciosControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.SeedDatabase();
        }

        [Fact]
        public async Task GetAll_ReturnsServiciosSembrados()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/servicios");

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<JsonElement>>(responseString);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task Create_ConPermisoDeAdministrador_ReturnsId()
        {
            var client = _factory.CreateClient();

            var body = JsonSerializer.Serialize(new
            {
                nombre = "Prueba-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                precio = 99.00m,
                duracionMinutos = 30,
                descripcion = "Servicio creado desde el test de integración",
                imagenUrl = "https://picsum.photos/seed/test/300/200",
                // Especialidad sembrada: Tratamientos Faciales
                especialidadId = "01a0bc01-2484-76f6-add2-b68394c7f1ac"
            });
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/servicios", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var id = JsonSerializer.Deserialize<Guid>(responseString);
            Assert.NotEqual(Guid.Empty, id);
        }
    }
}
