using AquaClinic.Application.Responses;

namespace AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad
{
    public class CreateEspecialidadCommandResponse: BaseResponse
    {
        public CreateEspecialidadCommandResponse(): base()
        {

        }

        public CreateEspecialidadDto Especialidad { get; set; } = default!;
    }
}