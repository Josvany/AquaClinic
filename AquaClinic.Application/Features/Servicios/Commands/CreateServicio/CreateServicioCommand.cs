using MediatR;

namespace AquaClinic.Application.Features.Servicios.Commands.CreateServicio
{
    public class CreateServicioCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public Guid EspecialidadId { get; set; }

        public override string ToString()
        {
            return $"Servicio: {Nombre}; Precio: {Precio}; Duración: {DuracionMinutos} min";
        }
    }
}
