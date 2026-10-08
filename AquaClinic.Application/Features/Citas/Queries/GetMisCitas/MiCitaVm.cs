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

        /// <summary>
        /// Valor numerico del estado (1=Pendiente, 2=Confirmada). La App lo
        /// usa para decidir si la cita admite cambio de horario.
        /// </summary>
        public int EstadoValor { get; set; }

        /// <summary>
        /// Duracion en minutos segun el servicio contratado; la App la
        /// necesita para recalcular el fin al mover la cita.
        /// </summary>
        public int DuracionMinutos { get; set; } = 60;
    }
}
