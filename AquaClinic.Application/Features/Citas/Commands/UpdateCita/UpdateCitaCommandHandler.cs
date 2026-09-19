using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.UpdateCita
{
    public class UpdateCitaCommandHandler : IRequestHandler<UpdateCitaCommand>
    {
        private readonly IAsyncRepository<Cita> _citaRepository;
        private readonly IMapper _mapper;

        public UpdateCitaCommandHandler(IMapper mapper, IAsyncRepository<Cita> citaRepository)
        {
            _mapper = mapper;
            _citaRepository = citaRepository;
        }

        public async Task Handle(UpdateCitaCommand request, CancellationToken cancellationToken)
        {

            var citaToUpdate = await _citaRepository.GetByIdAsync(request.CitaId);
            
            if (citaToUpdate == null) 
            {
                throw new NotFoundException(nameof(Cita), request.CitaId);
            }

            var validator = new UpdateCitaCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            _mapper.Map(request, citaToUpdate, typeof(UpdateCitaCommand), typeof(Cita));

            await _citaRepository.UpdateAsync(citaToUpdate);
        }
    }
}