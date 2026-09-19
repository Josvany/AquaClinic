using AutoMapper;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Models.Mail;
using AquaClinic.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Application.Features.Citas.Commands.CreateCita
{
    public class CreateCitaCommandHandler : IRequestHandler<CreateCitaCommand, Guid>
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateCitaCommandHandler> _logger;


        public CreateCitaCommandHandler(IMapper mapper, ICitaRepository citaRepository, IEmailService emailService, ILogger<CreateCitaCommandHandler> logger)
        {
            _mapper = mapper;
            _citaRepository = citaRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCitaCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateCitaCommandValidator(_citaRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new Exceptions.ValidationException(validationResult);

            var @cita = _mapper.Map<Cita>(request);


            @cita = await _citaRepository.AddAsync(@cita);


            var email = new Email() { To = "admin@aquaclinic.com", Body = $"Se creó una nueva cita: {request}", Subject = "Se creó una nueva cita" };

            try
            {
                await _emailService.SendEmail(email);
            }
            catch (Exception ex)
            {
                //this shouldn't stop the API from doing else so this can be logged
                _logger.LogError($"El envío de correo sobre la cita {@cita.CitaId} falló por un error en el servicio de correo: {ex.Message}");
            }

            return @cita.CitaId;
        }
    }
}
