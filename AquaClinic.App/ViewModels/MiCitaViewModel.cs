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

        /// <summary>Valor numerico del estado (1=Pendiente, 2=Confirmada).</summary>
        public int EstadoValor { get; set; }

        /// <summary>Duracion del servicio contratado, en minutos.</summary>
        public int DuracionMinutos { get; set; } = 60;

        /// <summary>
        /// True cuando la cita sigue vigente: no ha pasado la fecha de atencion
        /// y su estado permite gestionarla (pendiente o confirmada). Las citas
        /// completadas, canceladas o ya pasadas no admiten cambios.
        /// </summary>
        public bool PermiteGestionar => EstadoValor is 1 or 2 && Fecha > DateTime.Now;
    }
}
