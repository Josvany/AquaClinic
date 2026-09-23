using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Pages
{
    public partial class MiPerfil
    {
        [Inject]
        public IUsuarioDataService UsuarioDataService { get; set; } = default!;

        [Inject]
        public UsuarioSaludoCache SaludoCache { get; set; } = default!;

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

                // El saludo del nav se refresca al instante (misma sesion)
                if (Usuario is not null)
                {
                    SaludoCache.Establecer(
                        Usuario.Email,
                        UsuarioSaludoCache.DerivarNombre(Usuario.Email, Usuario.Nombre));
                }
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
