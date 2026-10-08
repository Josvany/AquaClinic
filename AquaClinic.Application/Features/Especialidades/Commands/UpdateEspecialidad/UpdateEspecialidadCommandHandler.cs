using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Commands.UpdateEspecialidad
{
    public class UpdateEspecialidadCommandHandler
        : IRequestHandler<UpdateEspecialidadCommand, UpdateEspecialidadCommandResponse>
    {
        private readonly IEspecialidadRepository _especialidadRepository;
        private readonly IMapper _mapper;

        public UpdateEspecialidadCommandHandler(IMapper mapper, IEspecialidadRepository especialidadRepository)
        {
            _mapper = mapper;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<UpdateEspecialidadCommandResponse> Handle(
            UpdateEspecialidadCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateEspecialidadCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
            {
                throw new ValidationException(validationResult);
            }

            var especialidad = await _especialidadRepository.GetByIdAsync(request.EspecialidadId);

            if (especialidad is null)
            {
                throw new NotFoundException(nameof(Especialidad), request.EspecialidadId);
            }

            // Sin validacion de nombre duplicado, igual que en CreateEspecialidad:
            // mantener el mismo criterio en altas y ediciones.
            _mapper.Map(request, especialidad, typeof(UpdateEspecialidadCommand), typeof(Especialidad));

            await _especialidadRepository.UpdateAsync(especialidad);

            return new UpdateEspecialidadCommandResponse
            {
                Success = true,
                Message = $"Especialidad \"{especialidad.Nombre}\" actualizada.",
                Especialidad = _mapper.Map<CreateEspecialidadDto>(especialidad)
            };
        }
    }
}