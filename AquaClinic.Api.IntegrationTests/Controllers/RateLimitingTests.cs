using AquaClinic.Api.IntegrationTests.Base;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AquaClinic.Api.IntegrationTests.Controllers
{
    /// <summary>
    /// Verifica que el rate limiter aplica el l&#237;mite estricto de autenticaci&#243;n
    /// (10 peticiones por ventana de 60s, por usuario/IP) sobre /login.
    /// </summary>
    public class RateLimitingTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public RateLimitingTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WhenLimitIsExceeded_Returns429()
        {
            // Arrange: cliente anonimo => particion "auth:" por IP/anonimo,
            // con limite de 10 peticiones por ventana (appsettings por defecto)
            var client = _factory.CreateClient();

            var content = new StringContent(
                JsonSerializer.Serialize(new { email = "inexistente@aquaclinic.com", password = "ClaveIncorrecta1!" }),
                Encoding.UTF8,
                "application/json");

            // Act: 10 peticiones dentro del limite + la 11 fuera
            HttpStatusCode ultimaRespuesta = HttpStatusCode.OK;
            for (int i = 0; i < 11; i++)
            {
                using var response = await client.PostAsync("/login?useCookies=true", content);
                ultimaRespuesta = response.StatusCode;
            }

            // Assert: las primeras 10 pasan el rate limiter (aunque el login falle con 401/400),
            // la 11 es rechazada por el limitador antes de ejecutar el endpoint
            Assert.Equal(HttpStatusCode.TooManyRequests, ultimaRespuesta);
        }
    }
}
