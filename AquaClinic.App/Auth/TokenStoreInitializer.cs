using System.Net.Http.Json;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Puente entre la sesion JWT del servidor (indexada por el ticket en la
    /// cookie del navegador) y el TokenStore scoped del circuito. Al montar el
    /// primer componente interactivo, recupera la sesion del ticket y la copia
    /// al TokenStore; luego notifica al AuthenticationStateProvider.
    /// </summary>
    public sealed class TokenStoreInitializer(
        TokenStore tokenStore,
        ApiAuthenticationStateProvider authStateProvider,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider) : IDisposable
    {
        private bool _ejecutado;

        /// <summary>
        /// Debe llamarse una vez por circuito, en el primer render interactivo.
        /// Restaura los tokens desde la sesion del servidor si el navegador
        /// trae el ticket en la cookie.
        /// </summary>
        public Task InicializarAsync()
        {
            if (_ejecutado)
            {
                return Task.CompletedTask;
            }

            _ejecutado = true;

            if (tokenStore.TieneToken)
            {
                return Task.CompletedTask;
            }

            var ticket = httpContextAccessor.HttpContext?.Request.Cookies[SesionPendiente.NombreCookie];
            var sesion = SesionPendiente.TomarPorTicket(ticket);

            if (sesion is null || string.IsNullOrEmpty(sesion.AccessToken))
            {
                return Task.CompletedTask;
            }

            tokenStore.Guardar(
                sesion.AccessToken,
                sesion.RefreshToken,
                sesion.ExpiraUtc,
                sesion.Usuario.Email);

            authStateProvider.InvalidarCache();

            // El saludo del nav muestra el nombre real del perfil
            return PrecalentarSaludoAsync(sesion.Usuario.Email);
        }

        private async Task PrecalentarSaludoAsync(string email)
        {
            try
            {
                using var client = serviceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient("ApiAuth");

                var perfil = await client.GetFromJsonAsync<PerfilDto>("api/usuarios/actual");

                if (perfil is not null && !string.IsNullOrWhiteSpace(perfil.Nombre))
                {
                    serviceProvider.GetRequiredService<Services.UsuarioSaludoCache>()
                        .Establecer(email, perfil.Nombre);
                }
            }
            catch
            {
                // Sin perfil disponible el saludo usa el nombre derivado del correo
            }
        }

        public void Dispose() { }

        private sealed record PerfilDto(
            string UsuarioId, string Email, string? Nombre, string? Apellidos, string? Telefono);
    }
}
