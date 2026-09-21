using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// Pagina de login estatica (SSR). El form POSTea al endpoint minimal API
    /// /auth/login que llama a la API, copia la Set-Cookie al navegador y
    /// hace redirect. Sin EditForm ni NavigationManager (evita el pipeline
    /// de Blazor enhanced forms que causa errores de navegacion).
    /// </summary>
    public partial class Login
    {
        [Parameter]
        public string? Error { get; set; }

        private string? ErrorMessage => Error switch
        {
            "credenciales" => "Correo y/o contrase\u00f1a incorrectos.",
            "ratelimit" => "Demasiados intentos. Espera un momento.",
            "api" => "No se pudo conectar con la API. Verifica que est\u00e9 corriendo.",
            _ => null
        };
    }
}
