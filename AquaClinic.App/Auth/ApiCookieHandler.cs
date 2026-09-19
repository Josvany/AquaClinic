using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Adjunta la cookie de la API (almacenada por usuario) a las peticiones salientes
    /// hacia los endpoints protegidos de la API. Resuelto por circuito, por lo que
    /// el proveedor de estado de autenticaci&#243;n corresponde al usuario actual.
    /// </summary>
    public class ApiCookieHandler : DelegatingHandler
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly UserCookieStore _cookieStore;

        public ApiCookieHandler(AuthenticationStateProvider authenticationStateProvider, UserCookieStore cookieStore)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _cookieStore = cookieStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is not null)
            {
                var cookie = _cookieStore.Get(userId);
                if (!string.IsNullOrEmpty(cookie))
                {
                    request.Headers.Add("Cookie", cookie);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
