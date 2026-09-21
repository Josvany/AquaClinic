using AutoMapper;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Models.Mail;
using AquaClinic.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Application.Features.Servicios.Commands.CreateServicio
{
    public class CreateServicioCommandHandler : IRequestHandler<CreateServicioCommand, Guid>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateServicioCommandHandler> _logger;

        public CreateServicioCommandHandler(IMapper mapper, IServicioRepository servicioRepository, IEmailService emailService, ILogger<CreateServicioCommandHandler> logger)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateServicioCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateServicioCommandValidator(_servicioRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new Exceptions.ValidationException(validationResult);

            var servicio = _mapper.Map<Servicio>(request);

            servicio = await _servicioRepository.AddAsync(servicio);

            var email = new Email() { To = "admin@aquaclinic.com", Body = $"Se creó un nuevo servicio: {request}", Subject = "Se creó un nuevo servicio" };

            try
            {
                await _emailService.SendEmail(email);
            }
            catch (Exception ex)
            {
                // No debe detener la operacion; queda registrado para revision
                _logger.LogError("El envío de correo sobre el servicio {ServicioId} falló por un error en el servicio de correo: {Mensaje}", servicio.ServicioId, ex.Message);
            }

            return servicio.ServicioId;
        }
    }
}
