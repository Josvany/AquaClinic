using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Promociones.Commands.DeletePromocion
{
    public class DeletePromocionCommandHandler : IRequestHandler<DeletePromocionCommand>
    {
        private readonly IPromocionRepository _promocionRepository;

        public DeletePromocionCommandHandler(IPromocionRepository promocionRepository)
        {
            _promocionRepository = promocionRepository;
        }

        public async Task Handle(DeletePromocionCommand request, CancellationToken cancellationToken)
        {
            var promocionToDelete = await _promocionRepository.GetByIdAsync(request.PromocionId)
                ?? throw new NotFoundException(nameof(Promocion), request.PromocionId);

            await _promocionRepository.DeleteAsync(promocionToDelete);
        }
    }
}