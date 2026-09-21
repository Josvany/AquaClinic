using AquaClinic.Api.IntegrationTests.Base;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AquaClinic.Api.IntegrationTests.Controllers
{
    /// <summary>
    /// Verifica que el rate limiter aplica el limite estricto de autenticacion
    /// (10 peticiones por ventana de 60s, por usuario/IP) sobre /login.
    /// Coleccion propia (factory separado): agota la particion de rate limiting
    /// y no debe contaminar a los demas tests de integracion.
    /// </summary>
    [Collection("RateLimiting")]
    public class RateLimitingTests : IClassFixture<RateLimitTestWebApplicationFactory<Program>>
    {
        private readonly RateLimitTestWebApplicationFactory<Program> _factory;

        public RateLimitingTests(RateLimitTestWebApplicationFactory<Program> factory)
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
