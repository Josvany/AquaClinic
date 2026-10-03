using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Commands.CreateBloqueo
{
    public class CreateBloqueoCommandHandler : IRequestHandler<CreateBloqueoCommand, Guid>
    {
        private readonly IBloqueoRepository _bloqueoRepository;
        private readonly IMapper _mapper;

        public CreateBloqueoCommandHandler(IMapper mapper, IBloqueoRepository bloqueoRepository)
        {
            _mapper = mapper;
            _bloqueoRepository = bloqueoRepository;
        }

        public async Task<Guid> Handle(CreateBloqueoCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateBloqueoCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            var bloqueo = _mapper.Map<Bloqueo>(request);
            bloqueo.PersonalId = null; // este CRUD cierra la clinica completa

            bloqueo = await _bloqueoRepository.AddAsync(bloqueo);

            return bloqueo.BloqueoId;
        }
    }
}
