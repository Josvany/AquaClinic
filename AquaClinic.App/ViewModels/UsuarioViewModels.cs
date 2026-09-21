namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Perfil del usuario devuelto por la API (GET /api/usuarios/actual y POST login).
    /// </summary>
    public class UsuarioViewModel
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Telefono { get; set; }
        public List<string> Roles { get; set; } = [];
    }

    /// <summary>
    /// Request de cambio de contrase&#241;a.
    /// </summary>
    public class CambiarPasswordViewModel
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
        public string PasswordNuevaConfirmacion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request de restablecimiento de contrase&#241;a con token.
    /// </summary>
    public class RestablecerPasswordViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
    }
}
