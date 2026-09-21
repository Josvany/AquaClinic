namespace AquaClinic.Application.Features.Usuarios
{
    /// <summary>
    /// Perfil del usuario devuelto por login y consultas de cuenta.
    /// </summary>
    public class UsuarioVm
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Telefono { get; set; }
        public IReadOnlyList<string> Roles { get; set; } = [];
    }
}
