using System.Net;
using System.Net.Http.Json;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Reemplaza al CookieForwardingHandler: inyecta el access token JWT del
    /// TokenStore como Authorization: Bearer en cada request hacia la API.
    /// Si la API responde 401 (token vencido/revocado) y hay refresh token,
    /// llama a api/usuarios/refresh (que rota el token), actualiza el store
    /// y reintenta la peticion original una sola vez.
    /// </summary>
    public class AuthHeaderHandler(TokenStore tokenStore, IServiceProvider serviceProvider) : DelegatingHandler
    {
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Los endpoints de auth van sin Authorization y sin reintento
            var esEndpointAuth = request.RequestUri?.AbsolutePath
                .Contains("/usuarios/login", StringComparison.OrdinalIgnoreCase) == true
                || request.RequestUri?.AbsolutePath
                .Contains("/usuarios/refresh", StringComparison.OrdinalIgnoreCase) == true;

            if (!esEndpointAuth && tokenStore.TieneToken)
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);
            }

            var respuesta = await base.SendAsync(request, cancellationToken);

            if (respuesta.StatusCode != HttpStatusCode.Unauthorized
                || esEndpointAuth
                || string.IsNullOrEmpty(tokenStore.RefreshToken))
            {
                return respuesta;
            }

            // 401 con refresh disponible: refrescar y reintentar una vez
            if (await RefrescarAsync(cancellationToken))
            {
                var reintento = await ClonarAsync(request, cancellationToken);

                if (reintento is not null)
                {
                    reintento.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);

                    var segunda = await base.SendAsync(reintento, cancellationToken);
                    return segunda;
                }
            }
            else
            {
                // Refresh invalido: sesion terminada
                tokenStore.Limpiar();
                NotificarSesionTerminada();
            }

            return respuesta;
        }

        /// <summary>Rota el refresh token y actualiza el store. Serializado por circuito.</summary>
        private async Task<bool> RefrescarAsync(CancellationToken cancellationToken)
        {
            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                // Otro request pudo haber refrescado mientras esperabamos el lock
                if (string.IsNullOrEmpty(tokenStore.RefreshToken))
                {
                    return false;
                }

                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                using var client = httpClientFactory.CreateClient("ApiAuth");

                using var peticion = new HttpRequestMessage(HttpMethod.Post, "api/usuarios/refresh")
                {
                    Content = JsonContent.Create(new { refreshToken = tokenStore.RefreshToken })
                };

                using var respuesta = await client.SendAsync(peticion, cancellationToken);

                if (!respuesta.IsSuccessStatusCode)
                {
                    return false;
                }

                var datos = await respuesta.Content
                    .ReadFromJsonAsync<RespuestaLogin>(cancellationToken);

                if (datos is null || string.IsNullOrEmpty(datos.AccessToken))
                {
                    return false;
                }

                tokenStore.ActualizarAccessToken(datos.AccessToken, datos.ExpiraUtc);
                tokenStore.GuardarRefresh(datos.RefreshToken);
                return true;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private void NotificarSesionTerminada()
        {
            try
            {
                serviceProvider.GetRequiredService<ApiAuthenticationStateProvider>().InvalidarCache();
            }
            catch (ObjectDisposedException)
            {
                // El circuito se esta cerrando: nada que notificar
            }
        }

        /// <summary>Clona el request original para el reintento (el body no se puede leer dos veces).</summary>
        private static async Task<HttpRequestMessage?> ClonarAsync(
            HttpRequestMessage original, CancellationToken cancellationToken)
        {
            var clon = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version
            };

            if (original.Content is not null)
            {
                var bytes = await original.Content.ReadAsByteArrayAsync(cancellationToken);
                clon.Content = new ByteArrayContent(bytes);

                foreach (var header in original.Content.Headers)
                {
                    clon.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var header in original.Headers)
            {
                if (!string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    clon.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var option in original.Options)
            {
                clon.Options.TryAdd(option.Key, option.Value);
            }

            return clon;
        }

        // DTO de la respuesta de login/refresh de la API
        private sealed record RespuestaLogin(
            string AccessToken,
            string RefreshToken,
            DateTime ExpiraUtc,
            UsuarioDto Usuario);

        private sealed record UsuarioDto(
            string UsuarioId,
            string Email,
            string? Nombre,
            string? Apellidos,
            string? Telefono,
            IReadOnlyList<string> Roles);
    }
}
