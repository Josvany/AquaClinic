using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Commands.UpdateEspecialidad
{
    /// <summary>Modifica el nombre de una especialidad existente.</summary>
    public class UpdateEspecialidadCommand : IRequest<UpdateEspecialidadCommandResponse>
    {
        public Guid EspecialidadId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Especialidad: {EspecialidadId} -> {Nombre}";
        }
    }
}