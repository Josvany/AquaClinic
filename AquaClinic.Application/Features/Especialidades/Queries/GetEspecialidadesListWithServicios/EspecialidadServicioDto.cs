using System;
using System.Collections.Generic;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithServicios
{
    public class EspecialidadServicioDto
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
        public Guid EspecialidadId { get; set; }
    }
}
