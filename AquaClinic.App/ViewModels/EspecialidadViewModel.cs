namespace AquaClinic.App.ViewModels
{
    /// <summary>Especialidad del catalogo para el CRUD administrativo.</summary>
    public class EspecialidadViewModel
    {
        public Guid EspecialidadId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Cuantos servicios tiene asignados. Solo informativo en el listado: la
        /// API no permite borrar una especialidad con servicios.
        /// </summary>
        public int CantidadServicios { get; set; }
    }
}