using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad
{
    public class CreateEspecialidadCommandHandler : IRequestHandler<CreateEspecialidadCommand, CreateEspecialidadCommandResponse>
    {
        private readonly IAsyncRepository<Especialidad> _especialidadRepository;
        private readonly IMapper _mapper;

        public CreateEspecialidadCommandHandler(IMapper mapper, IAsyncRepository<Especialidad> especialidadRepository)
        {
            _mapper = mapper;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<CreateEspecialidadCommandResponse> Handle(CreateEspecialidadCommand request, CancellationToken cancellationToken)
        {
            var createEspecialidadCommandResponse = new CreateEspecialidadCommandResponse();

            var validator = new CreateEspecialidadCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
            {
                createEspecialidadCommandResponse.Success = false;
                createEspecialidadCommandResponse.ValidationErrors = new List<string>();
                foreach (var error in validationResult.Errors)
                {
                    createEspecialidadCommandResponse.ValidationErrors.Add(error.ErrorMessage);
                }
            }
            if (createEspecialidadCommandResponse.Success)
            {
                var category = new Especialidad() { Nombre = request.Nombre };
                category = await _especialidadRepository.AddAsync(category);
                createEspecialidadCommandResponse.Especialidad = _mapper.Map<CreateEspecialidadDto>(category);
            }

            return createEspecialidadCommandResponse;
        }
    }
}
