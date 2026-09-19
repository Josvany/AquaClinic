
using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    public class Cita : AuditableEntity
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Precio { get; set; }
        public string? Doctor { get; set; }
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public Guid EspecialidadId { get; set; }
        public Especialidad Especialidad { get; set; } = default!;

    }
}
