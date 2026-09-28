namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Cita del propio usuario para la pantalla "Mis citas" (/miscitas).
    /// Espejo de MiCitaVm de la API.
    /// </summary>
    public class MiCitaViewModel
    {
        public Guid CitaId { get; set; }
        public string Servicio { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Notas { get; set; }
    }
}
