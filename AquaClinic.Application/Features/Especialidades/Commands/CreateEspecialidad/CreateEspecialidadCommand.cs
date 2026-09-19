using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad
{
    public class CreateEspecialidadCommand: IRequest<CreateEspecialidadCommandResponse>
    {
        public string Nombre { get; set; } = string.Empty;
    }
}
