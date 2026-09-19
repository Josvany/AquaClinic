using MediatR;
using System;

namespace AquaClinic.Application.Features.Citas.Commands.UpdateCita
{
    public class UpdateCitaCommand : IRequest
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; }
        public int Precio { get; set; }
        public string Doctor { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public Guid EspecialidadId { get; set; }
    }
}
