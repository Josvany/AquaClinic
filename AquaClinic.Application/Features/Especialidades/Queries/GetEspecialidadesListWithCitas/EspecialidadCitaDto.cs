using System;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithCitas
{
    public class EspecialidadCitaDto
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; }
        public int Precio { get; set; }
        public string Doctor { get; set; }
        public DateTime Fecha { get; set; }
        public Guid EspecialidadId { get; set; }
    }
}
