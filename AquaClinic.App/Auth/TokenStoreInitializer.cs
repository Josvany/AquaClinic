namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Puente entre la sesion cifrada de la cookie y el TokenStore scoped del
    /// circuito. Al montar el primer componente interactivo, descifra la cookie y
    /// copia los tokens al TokenStore; luego notifica al AuthenticationStateProvider.
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
        /// Restaura los tokens desde la cookie del navegador.
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

            var httpContext = httpContextAccessor.HttpContext;
            var sesiones = serviceProvider.GetRequiredService<SesionCookieStore>();
            var sesion = sesiones.Descifrar(httpContext?.Request.Cookies[SesionCookieStore.NombreCookie]);

            if (sesion is null || string.IsNullOrEmpty(sesion.AccessToken))
            {
                return Task.CompletedTask;
            }

            // El correo no viaja en la cookie: sale del propio access token.
            var email = SesionCookieStore.LeerEmail(sesion.AccessToken) ?? string.Empty;

            tokenStore.Guardar(
                sesion.AccessToken,
                sesion.RefreshToken,
                sesion.ExpiraUtc,
                email);

            authStateProvider.InvalidarCache();

            // El saludo del nav muestra el nombre real del perfil
            return PrecalentarSaludoAsync(email);
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