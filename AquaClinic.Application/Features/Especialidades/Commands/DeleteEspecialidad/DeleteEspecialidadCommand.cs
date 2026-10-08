using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Commands.DeleteEspecialidad
{
    /// <summary>Elimina una especialidad del catalogo.</summary>
    public class DeleteEspecialidadCommand : IRequest
    {
        public Guid EspecialidadId { get; set; }

        public override string ToString()
        {
            return $"Especialidad: {EspecialidadId}";
        }
    }
}