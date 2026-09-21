using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Categoria de servicios de la clinica (ej. Tratamientos Faciales,
    /// Cuidado Avanzado de la Piel, Inyectables, Cejas).
    /// </summary>
    public class Especialidad : AuditableEntity
    {
        public Especialidad()
        {
            EspecialidadId = Guid.CreateVersion7();
        }

        public Guid EspecialidadId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public ICollection<Servicio>? Servicios { get; set; }
    }
}
