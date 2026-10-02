using AquaClinic.App.Localization;
using System.ComponentModel.DataAnnotations;

namespace AquaClinic.App.ViewModels
{
    public class RegisterViewModel
    {
        [RequiredUi("El correo es obligatorio")]
        [EmailAddressUi("Ingresa un correo válido")]
        public string Email { get; set; } = string.Empty;

        [RequiredUi("La contraseña es obligatoria")]
        [MinLengthUi(8, "Debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
