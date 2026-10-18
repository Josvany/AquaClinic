using MediatR;

namespace AquaClinic.Application.Features.Usuarios.Registrarse
{
    public class RegistrarseCommand : IRequest<string>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Telefono { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public bool AceptaComunicaciones { get; set; }
    }
}
