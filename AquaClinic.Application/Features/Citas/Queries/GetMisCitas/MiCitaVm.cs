namespace AquaClinic.Application.Features.Citas.Queries.GetMisCitas
{
    /// <summary>
    /// Cita del propio usuario para la pantalla "Mis citas" de la App:
    /// servicio contratado, fecha local, estado y notas.
    /// </summary>
    public class MiCitaVm
    {
        public Guid CitaId { get; set; }

        /// <summary>Nombre del servicio contratado (el primero si hay varios).</summary>
        public string Servicio { get; set; } = string.Empty;

        /// <summary>Precio en CAD del servicio contratado.</summary>
        public decimal Precio { get; set; }

        public DateTime Fecha { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string? Notas { get; set; }
    }
}
