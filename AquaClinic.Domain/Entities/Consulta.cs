
using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    public class Consulta : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int TotalConsulta { get; set; }
        public DateTime FechaSolicitada { get; set; }
        public bool Pagada { get; set; }
    }
}
