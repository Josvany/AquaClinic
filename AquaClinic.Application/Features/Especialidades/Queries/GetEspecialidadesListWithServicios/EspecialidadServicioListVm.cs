using System;
using System.Collections.Generic;

namespace AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithServicios
{
    public class EspecialidadServicioListVm
    {
        public Guid EspecialidadId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<EspecialidadServicioDto> Servicios { get; set; } = default!;
    }
}
