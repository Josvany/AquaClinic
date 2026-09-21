using AquaClinic.Api.IntegrationTests.Base;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList;
using System.Text.Json;

namespace AquaClinic.Api.IntegrationTests.Controllers
{

[Collection("ApiIntegration")]
    public class EspecialidadesControllerTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public EspecialidadesControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.SeedDatabase();
        }

        [Fact]
        public async Task ReturnsSuccessResult()
        {
            var client = _factory.GetAnonymousClient();

            var response = await client.GetAsync("/api/especialidades/all");

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<List<EspecialidadListVm>>(responseString);
            
            Assert.IsType<List<EspecialidadListVm>>(result);
            Assert.NotEmpty(result);
        }
    }
}
