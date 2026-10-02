using AquaClinic.App.Localization;
using System.ComponentModel.DataAnnotations;

namespace AquaClinic.App.ViewModels
{
    public class LoginViewModel
    {
        [RequiredUi("El correo es obligatorio")]
        [EmailAddressUi("Ingresa un correo válido")]
        public string Email { get; set; } = string.Empty;

        [RequiredUi("La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }
}
