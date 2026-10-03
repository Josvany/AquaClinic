using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Commands.UpdateBloqueo
{
    public class UpdateBloqueoCommandHandler : IRequestHandler<UpdateBloqueoCommand>
    {
        private readonly IBloqueoRepository _bloqueoRepository;
        private readonly IMapper _mapper;

        public UpdateBloqueoCommandHandler(IMapper mapper, IBloqueoRepository bloqueoRepository)
        {
            _mapper = mapper;
            _bloqueoRepository = bloqueoRepository;
        }

        public async Task Handle(UpdateBloqueoCommand request, CancellationToken cancellationToken)
        {
            var bloqueoToUpdate = await _bloqueoRepository.GetByIdAsync(request.BloqueoId);

            if (bloqueoToUpdate == null)
            {
                throw new NotFoundException(nameof(Bloqueo), request.BloqueoId);
            }

            var validator = new UpdateBloqueoCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            _mapper.Map(request, bloqueoToUpdate, typeof(UpdateBloqueoCommand), typeof(Bloqueo));

            await _bloqueoRepository.UpdateAsync(bloqueoToUpdate);
        }
    }
}
