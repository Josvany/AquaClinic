namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Cliente registrado (rol Cliente) que se puede seleccionar al agendar una
    /// cita desde el back office.
    /// </summary>
    public class ClienteBusquedaViewModel
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }

        /// <summary>Texto que se muestra en el selector.</summary>
        public string Etiqueta => string.IsNullOrWhiteSpace(Nombre) ? Email : $"{Nombre} · {Email}";
    }
}
