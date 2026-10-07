using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Promociones.Commands.CreatePromocion
{
    public class CreatePromocionCommandHandler : IRequestHandler<CreatePromocionCommand, Guid>
    {
        private readonly IPromocionRepository _promocionRepository;
        private readonly IMapper _mapper;

        public CreatePromocionCommandHandler(IMapper mapper, IPromocionRepository promocionRepository)
        {
            _mapper = mapper;
            _promocionRepository = promocionRepository;
        }

        public async Task<Guid> Handle(CreatePromocionCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreatePromocionCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            var promocion = _mapper.Map<Promocion>(request);

            promocion = await _promocionRepository.AddAsync(promocion);

            // Alcance: vacio deja la promocion global (aplica a todos).
            await _promocionRepository.ReemplazarServiciosAsync(promocion, request.ServicioIds);

            return promocion.PromocionId;
        }
    }
}