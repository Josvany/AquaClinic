namespace AquaClinic.Application.Features.Citas.Queries.GetCitaById
{
    /// <summary>
    /// Detalle de una cita para la pantalla /citadetails de la agenda interna.
    /// El contrato (citaId, nombre, precio, doctor, fecha, descripcion, imagenUrl,
    /// especialidadId) es el que consume el cliente tipado de la UI.
    /// </summary>
    public class CitaDetailVm
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Precio { get; set; }
        public string? Doctor { get; set; }
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public Guid EspecialidadId { get; set; }
        public CitaDetalleEspecialidadDto? Especialidad { get; set; }
    }

    public class CitaDetalleEspecialidadDto
    {
        public Guid EspecialidadId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
