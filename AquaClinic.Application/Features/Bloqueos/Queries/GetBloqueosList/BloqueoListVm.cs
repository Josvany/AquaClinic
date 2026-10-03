namespace AquaClinic.Application.Features.Bloqueos.Queries.GetBloqueosList
{
    /// <summary>
    /// Bloqueo de toda la clinica (cierre, feriado, capacitacion) para la tabla
    /// administrativa. Las fechas van en UTC, como en la base de datos; la App las
    /// convierte a hora local de la clinica al mostrarlas.
    /// </summary>
    public class BloqueoListVm
    {
        public Guid BloqueoId { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public DateTimeOffset FechaInicio { get; set; }

        public DateTimeOffset FechaFin { get; set; }
    }
}
