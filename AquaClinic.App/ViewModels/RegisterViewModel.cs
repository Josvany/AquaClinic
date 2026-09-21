using System.ComponentModel.DataAnnotations;

namespace AquaClinic.App.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un correo v\u00e1lido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "Debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
