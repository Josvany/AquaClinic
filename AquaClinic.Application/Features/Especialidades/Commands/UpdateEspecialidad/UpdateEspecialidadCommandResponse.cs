using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using AquaClinic.Application.Responses;

namespace AquaClinic.Application.Features.Especialidades.Commands.UpdateEspecialidad
{
    public class UpdateEspecialidadCommandResponse : BaseResponse
    {
        public UpdateEspecialidadCommandResponse()
        {
        }

        public CreateEspecialidadDto Especialidad { get; set; } = default!;
    }
}