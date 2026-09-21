using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Commands.UpdateServicio
{
    public class UpdateServicioCommandHandler : IRequestHandler<UpdateServicioCommand>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IMapper _mapper;

        public UpdateServicioCommandHandler(IMapper mapper, IServicioRepository servicioRepository)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
        }

        public async Task Handle(UpdateServicioCommand request, CancellationToken cancellationToken)
        {
            var servicioToUpdate = await _servicioRepository.GetByIdAsync(request.ServicioId);

            if (servicioToUpdate == null)
            {
                throw new NotFoundException(nameof(Servicio), request.ServicioId);
            }

            var validator = new UpdateServicioCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            _mapper.Map(request, servicioToUpdate, typeof(UpdateServicioCommand), typeof(Servicio));

            await _servicioRepository.UpdateAsync(servicioToUpdate);
        }
    }
}
