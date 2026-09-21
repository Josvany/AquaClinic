using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace AquaClinic.App.Pages
{
    public class CambiarPasswordModelo
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseñ es obligatoria")]
        [MinLength(8, ErrorMessage = "Debe tener al menos 8 caracteres")]
        public string PasswordNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la nueva contraseña")]
        [Compare(nameof(PasswordNueva), ErrorMessage = "Las contraseña no coinciden")]
        public string PasswordNuevaConfirmacion { get; set; } = string.Empty;
    }

    public partial class CambiarPassword
    {
        [Inject]
        public IUsuarioDataService UsuarioDataService { get; set; } = default!;

        public CambiarPasswordModelo Modelo { get; set; } = new();
        public string? Message { get; set; }
        public string? TokenRestablecimiento { get; set; }

        protected async Task HandleCambiar()
        {
            if (Modelo.PasswordNueva != Modelo.PasswordNuevaConfirmacion)
            {
                Message = "Las contraseña no coinciden.";
                return;
            }

            var respuesta = await UsuarioDataService.CambiarPasswordAsync(
                Modelo.PasswordActual, Modelo.PasswordNueva);

            if (respuesta.Success)
            {
                Message = "Contraseña actualizada correctamente.";
                Modelo = new CambiarPasswordModelo();
            }
            else
            {
                Message = respuesta.Message;
                if (!string.IsNullOrEmpty(respuesta.ValidationErrors))
                {
                    Message += " " + respuesta.ValidationErrors;
                }
            }
        }
    }
}
