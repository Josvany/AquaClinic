using Microsoft.AspNetCore.Http;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Reenvia la cookie del navegador (same-domain localhost) a las peticiones
    /// hacia la API. En SSR/prerender funciona porque HttpContext esta disponible
    /// con las cookies del request entrante. En el circuito interactivo, HttpContext
    /// es null pero la cookie queda cacheada por el ApiAuthenticationStateProvider.
    /// </summary>
    public class CookieForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext is not null)
            {
                var cookieHeader = httpContext.Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    request.Headers.Add("Cookie", cookieHeader);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
