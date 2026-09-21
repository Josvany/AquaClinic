using AquaClinic.App.Contracts;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Pages
{
    public partial class MiPerfil
    {
        [Inject]
        public IUsuarioDataService UsuarioDataService { get; set; } = default!;

        public UsuarioViewModel? Usuario { get; set; }

        public bool Cargando { get; set; } = true;
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Usuario = await UsuarioDataService.GetUsuarioActualAsync();
            Cargando = false;
        }

        protected async Task HandleGuardar()
        {
            MensajeExito = null;
            MensajeError = null;

            var respuesta = await UsuarioDataService.ActualizarPerfilAsync(
                Usuario!.Nombre, Usuario.Apellidos, Usuario.Telefono);

            if (respuesta.Success)
            {
                MensajeExito = "Perfil actualizado correctamente.";
                Usuario = await UsuarioDataService.GetUsuarioActualAsync();
            }
            else
            {
                MensajeError = respuesta.Message;
                if (!string.IsNullOrEmpty(respuesta.ValidationErrors))
                {
                    MensajeError += " " + respuesta.ValidationErrors;
                }
            }
        }
    }
}
