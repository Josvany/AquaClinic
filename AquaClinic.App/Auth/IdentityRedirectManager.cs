using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Redirecciones desde p&#225;ginas con renderizado est&#225;tico (Login / Registro).
    /// NavigationException es capturada por el framework durante la renderizaci&#243;n
    /// est&#225;tica y convertida en una redirecci&#243;n HTTP.
    /// </summary>
    public sealed class IdentityRedirectManager(NavigationManager navigationManager)
    {
        [DoesNotReturn]
        public void RedirectTo(string? uri = null)
        {
            uri ??= string.Empty;

            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
            {
                uri = navigationManager.ToAbsoluteUri(uri).PathAndQuery;
            }

            throw new NavigationException(uri);
        }
    }
}
