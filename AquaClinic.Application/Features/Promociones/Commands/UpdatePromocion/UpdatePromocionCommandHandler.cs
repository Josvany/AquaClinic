using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Promociones.Commands.UpdatePromocion
{
    public class UpdatePromocionCommandHandler : IRequestHandler<UpdatePromocionCommand>
    {
        private readonly IPromocionRepository _promocionRepository;
        private readonly IMapper _mapper;

        public UpdatePromocionCommandHandler(IMapper mapper, IPromocionRepository promocionRepository)
        {
            _mapper = mapper;
            _promocionRepository = promocionRepository;
        }

        public async Task Handle(UpdatePromocionCommand request, CancellationToken cancellationToken)
        {
            var promocionToUpdate = await _promocionRepository.GetByIdAsync(request.PromocionId);

            if (promocionToUpdate == null)
            {
                throw new NotFoundException(nameof(Promocion), request.PromocionId);
            }

            var validator = new UpdatePromocionCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            _mapper.Map(request, promocionToUpdate, typeof(UpdatePromocionCommand), typeof(Promocion));

            await _promocionRepository.UpdateAsync(promocionToUpdate);

            // Alcance: vacio deja la promocion global (aplica a todos).
            await _promocionRepository.ReemplazarServiciosAsync(promocionToUpdate, request.ServicioIds);
        }
    }
}