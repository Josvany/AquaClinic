using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.CreateCita
{
    public class CreateCitaCommand : IRequest<Guid>
    {
        public string Nombre { get; set; }
        public int Precio { get; set; }
        public string Doctor { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public Guid EspecialidadId { get; set; }
        public override string ToString()
        {
            return $"Cita: {Nombre}; Precio: {Precio}; Doctor: {Doctor}; Fecha: {Fecha.ToShortDateString()}; Descripción: {Descripcion}";
        }
    }
}
