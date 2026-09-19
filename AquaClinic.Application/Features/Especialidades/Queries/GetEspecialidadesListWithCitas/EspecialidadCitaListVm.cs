using System;
using System.Collections.Generic;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithCitas
{
    public class EspecialidadCitaListVm
    {
        public Guid EspecialidadId { get; set; }
        public string Nombre { get; set; }
        public ICollection<EspecialidadCitaDto> Citas { get; set; }
    }
}
