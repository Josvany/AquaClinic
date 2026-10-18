using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// Pagina de registro estatica (SSR). El form POSTea al endpoint minimal API
    /// /auth/register que registra contra la API, hace login automatico
    /// (copiando la cookie al navegador) y redirige.
    /// </summary>
    public partial class Register
    {
        [Parameter]
        public string? Error { get; set; }

        private string? ErrorMessage => Error switch
        {
            "duplicado" => "El correo ya est\u00e1 registrado.",
            "password" => "La contrase\u00f1a no cumple los requisitos (m\u00ednimo 8 caracteres).",
            "ratelimit" => "Demasiados intentos. Espera un momento.",
            "api" => "No se pudo conectar con la API. Verifica que esté corriendo.",
            "consentimiento" => "Debes aceptar recibir comunicaciones para crear la cuenta.",
            "edad" => "Debes confirmar que eres mayor de 19 años o cuentas con consentimiento de tu tutor legal.",
            _ => null
        };
    }
}