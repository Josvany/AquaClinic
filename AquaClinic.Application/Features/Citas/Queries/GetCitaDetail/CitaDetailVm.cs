using System;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitaDetail
{
    public class CitaDetailVm
    {
        public Guid CitaId { get; set; }

        public string Nombre { get; set; }

        public int Precio { get; set; }

        public string Doctor { get; set; }

        public DateTime Fecha { get; set; }

        public string Descripcion { get; set; }

        public string ImagenUrl { get; set; }

        public Guid EspecialidadId { get; set; }

        public EspecialidadDto Especialidad { get; set; }
    }
}