namespace AquaClinic.Api.IntegrationTests.Base
{
    /// <summary>
    /// Coleccion compartida para todos los tests de integracion de la API:
    /// los ejecuta secuencialmente (no en paralelo) y comparte un UNICO
    /// CustomWebApplicationFactory, evitando interferencias entre instancias
    /// InMemory y conflictos de estado compartido.
    /// </summary>
    [CollectionDefinition("ApiIntegration")]
    public class ApiIntegrationCollection : ICollectionFixture<CustomWebApplicationFactory<Program>>
    {
    }
}
