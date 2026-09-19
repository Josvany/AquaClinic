
using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    public class Especialidad: AuditableEntity
    {
        public Guid EspecialidadId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Cita>? Citas { get; set; }
    }
}
