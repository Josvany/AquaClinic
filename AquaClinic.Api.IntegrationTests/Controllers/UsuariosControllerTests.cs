using AquaClinic.Api.IntegrationTests.Base;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AquaClinic.Api.IntegrationTests.Controllers
{
    /// <summary>
    /// Flujo completo de la cuenta de un cliente contra la API en memoria:
    /// registro &#8594; login &#8594; perfil &#8594; cambio de contrase&#241;a &#8594; restablecimiento.
    /// El host de pruebas usa el esquema TestScheme, asi que para los endpoints
    /// [Authorize] se envia el id del usuario via el header X-Test-UserId.
    /// </summary>
[Collection("ApiIntegration")]
    public class UsuariosControllerTests
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public UsuariosControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            // Asegura roles de Identity (el MigrateAndSeedAsync del arranque no corre en Testing)
            _factory.SeedDatabase();
        }

        private static string CorreoUnico() => $"cliente-{Guid.NewGuid():N}@prueba.com";

        [Fact]
        public async Task FlujoClienteCompleto_RegistroLoginPerfilCambioYRestablecimiento()
        {
            // Arrange
            var client = _factory.CreateClient();
            var email = CorreoUnico();
            const string password = "ClaveInicial#2026";

            // 1) Registro: devuelve el UsuarioId (UUID v7 en string)
            var registro = new
            {
                email,
                password,
                nombre = "Cliente",
                apellidos = "De Prueba",
                telefono = "+1 604-555-0100"
            };
            var respuestaRegistro = await client.PostAsJsonAsync("/api/usuarios/registro", registro);
            Assert.Equal(HttpStatusCode.OK, respuestaRegistro.StatusCode);

            var usuarioId = await respuestaRegistro.Content.ReadAsStringAsync();
            Assert.NotEmpty(usuarioId);
            Assert.Equal(36, usuarioId.Trim('"').Length); // formato GUID v7

            // 2) Login: devuelve access token JWT + refresh token + perfil con rol Cliente
            var login = new { email, password };
            var respuestaLogin = await client.PostAsJsonAsync("/api/usuarios/login", login);
            Assert.Equal(HttpStatusCode.OK, respuestaLogin.StatusCode);

            var contenidoLogin = await respuestaLogin.Content.ReadAsStringAsync();
            using var documento = JsonDocument.Parse(contenidoLogin);
            Assert.False(string.IsNullOrWhiteSpace(documento.RootElement.GetProperty("accessToken").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(documento.RootElement.GetProperty("refreshToken").GetString()));
            Assert.Equal(email, documento.RootElement.GetProperty("usuario").GetProperty("email").GetString());
            Assert.Contains("Cliente", documento.RootElement.GetProperty("usuario").GetProperty("roles").EnumerateArray()
                .Select(r => r.GetString()));

            // 3) Login con password incorrecto -> 401
            var loginMalo = new { email, password = "Incorrecta#123" };
            var respuestaLoginMalo = await client.PostAsJsonAsync("/api/usuarios/login", loginMalo);
            Assert.Equal(HttpStatusCode.Unauthorized, respuestaLoginMalo.StatusCode);

            // 3b) Refresh: rota el refresh token y emite un nuevo access token
            var refreshToken = documento.RootElement.GetProperty("refreshToken").GetString();
            var respuestaRefresh = await client.PostAsJsonAsync("/api/usuarios/refresh", new { refreshToken });
            Assert.Equal(HttpStatusCode.OK, respuestaRefresh.StatusCode);

            var refreshDoc = JsonDocument.Parse(await respuestaRefresh.Content.ReadAsStringAsync());
            Assert.False(string.IsNullOrWhiteSpace(refreshDoc.RootElement.GetProperty("accessToken").GetString()));

            // El refresh token usado es de un solo uso: reusarlo -> 401
            var respuestaRefreshReusado = await client.PostAsJsonAsync("/api/usuarios/refresh", new { refreshToken });
            Assert.Equal(HttpStatusCode.Unauthorized, respuestaRefreshReusado.StatusCode);

            // 4) Perfil del usuario autenticado (via header X-Test-UserId)
            client.DefaultRequestHeaders.Add("X-Test-UserId", usuarioId.Trim('"'));
            var respuestaPerfil = await client.GetAsync("/api/usuarios/actual");
            Assert.Equal(HttpStatusCode.OK, respuestaPerfil.StatusCode);

            var perfil = JsonDocument.Parse(await respuestaPerfil.Content.ReadAsStringAsync());
            Assert.Equal("Cliente", perfil.RootElement.GetProperty("nombre").GetString());

            // 5) Actualizar perfil
            var actualizar = new { nombre = "NombreActualizado", apellidos = "ApellidosActualizados", telefono = "+1 604-555-0199" };
            var respuestaActualizar = await client.PutAsJsonAsync("/api/usuarios/actual", actualizar);
            Assert.Equal(HttpStatusCode.NoContent, respuestaActualizar.StatusCode);

            var respuestaPerfil2 = await client.GetAsync("/api/usuarios/actual");
            var perfil2 = JsonDocument.Parse(await respuestaPerfil2.Content.ReadAsStringAsync());
            Assert.Equal("NombreActualizado", perfil2.RootElement.GetProperty("nombre").GetString());

            // 6) Cambio de contrase&#241;a (verifica la actual)
            var cambiar = new { passwordActual = password, passwordNueva = "NuevaClave#2026" };
            var respuestaCambio = await client.PostAsJsonAsync("/api/usuarios/cambiar-password", cambiar);
            Assert.Equal(HttpStatusCode.NoContent, respuestaCambio.StatusCode);

            // 7) Login con la contrase&#241;a nueva funciona
            var loginNuevo = await client.PostAsJsonAsync("/api/usuarios/login", new { email, password = "NuevaClave#2026" });
            Assert.Equal(HttpStatusCode.OK, loginNuevo.StatusCode);

            // 8) Cambio con contrase&#241;a actual incorrecta -> 400
            var cambiarMalo = new { passwordActual = "Equivocada#1", passwordNueva = "OtraClave#2026" };
            var respuestaCambioMalo = await client.PostAsJsonAsync("/api/usuarios/cambiar-password", cambiarMalo);
            Assert.Equal(HttpStatusCode.BadRequest, respuestaCambioMalo.StatusCode);

            // 9) Olvide contrase&#241;a (fuera de Production expone el token)
            var olvide = new { email };
            var respuestaOlvide = await client.PostAsJsonAsync("/api/usuarios/olvide-password", olvide);
            Assert.Equal(HttpStatusCode.OK, respuestaOlvide.StatusCode);

            var olvideDoc = JsonDocument.Parse(await respuestaOlvide.Content.ReadAsStringAsync());
            var token = olvideDoc.RootElement.GetProperty("token").GetString();
            Assert.False(string.IsNullOrEmpty(token), "El token debe ser visible en el entorno de pruebas");

            // 10) Restablecer con el token y volver a iniciar sesion
            var restablecer = new { email, token, passwordNueva = "Restablecida#2026" };
            var respuestaRestablecer = await client.PostAsJsonAsync("/api/usuarios/restablecer-password", restablecer);
            Assert.Equal(HttpStatusCode.NoContent, respuestaRestablecer.StatusCode);

            var loginFinal = await client.PostAsJsonAsync("/api/usuarios/login", new { email, password = "Restablecida#2026" });
            Assert.Equal(HttpStatusCode.OK, loginFinal.StatusCode);
        }

        [Fact]
        public async Task Registro_ConCorreoYaRegistrado_Devuelve400()
        {
            var client = _factory.CreateClient();
            var email = CorreoUnico();
            var registro = new { email, password = "ClaveInicial#2026", nombre = "Duplicado" };

            var primera = await client.PostAsJsonAsync("/api/usuarios/registro", registro);
            Assert.Equal(HttpStatusCode.OK, primera.StatusCode);

            var segunda = await client.PostAsJsonAsync("/api/usuarios/registro", registro);
            Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
        }

        [Fact]
        public async Task GetUsuarioActual_ConUsuarioInexistente_Devuelve401()
        {
            // El TestAuthHandler autentica con un Id que no existe en la BD,
            // el handler retorna null y el controller responde 401 (re-login)
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-UserId", Guid.NewGuid().ToString());
            var respuesta = await client.GetAsync("/api/usuarios/actual");
            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }
    }
}
