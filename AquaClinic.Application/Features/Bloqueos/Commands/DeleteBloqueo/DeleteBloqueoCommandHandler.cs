using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Commands.DeleteBloqueo
{
    public class DeleteBloqueoCommandHandler : IRequestHandler<DeleteBloqueoCommand>
    {
        private readonly IBloqueoRepository _bloqueoRepository;

        public DeleteBloqueoCommandHandler(IBloqueoRepository bloqueoRepository)
        {
            _bloqueoRepository = bloqueoRepository;
        }

        public async Task Handle(DeleteBloqueoCommand request, CancellationToken cancellationToken)
        {
            var bloqueoToDelete = await _bloqueoRepository.GetByIdAsync(request.BloqueoId)
                ?? throw new NotFoundException(nameof(Bloqueo), request.BloqueoId);

            await _bloqueoRepository.DeleteAsync(bloqueoToDelete);
        }
    }
}
