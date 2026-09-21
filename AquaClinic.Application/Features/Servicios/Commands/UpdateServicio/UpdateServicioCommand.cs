using MediatR;

namespace AquaClinic.Application.Features.Servicios.Commands.UpdateServicio
{
    public class UpdateServicioCommand : IRequest
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public Guid EspecialidadId { get; set; }
    }
}
