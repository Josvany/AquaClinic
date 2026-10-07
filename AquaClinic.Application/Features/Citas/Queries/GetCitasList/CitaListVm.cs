namespace AquaClinic.Application.Features.Citas.Queries.GetCitasList
{
    /// <summary>
    /// Cita para el listado y el calendario de la App. El contrato
    /// (citaId, nombre, fecha, estado) es el que consume la App.
    /// </summary>
    public class CitaListVm
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        /// <summary>Estado legible (Pendiente/Confirmada/Cancelada/Completada).</summary>
        public string Estado { get; set; } = string.Empty;
    }
}
