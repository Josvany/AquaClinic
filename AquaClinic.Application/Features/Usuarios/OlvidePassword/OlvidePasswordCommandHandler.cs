using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Models.Mail;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Application.Features.Usuarios.OlvidePassword
{
    public class OlvidePasswordCommandHandler : IRequestHandler<OlvidePasswordCommand, OlvidePasswordResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OlvidePasswordCommandHandler> _logger;

        public OlvidePasswordCommandHandler(
            IIdentityService identityService,
            IEmailService emailService,
            ILogger<OlvidePasswordCommandHandler> logger)
        {
            _identityService = identityService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<OlvidePasswordResponse> Handle(OlvidePasswordCommand request, CancellationToken cancellationToken)
        {
            var validator = new OlvidePasswordCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new Exceptions.ValidationException(validationResult);

            var token = await _identityService.GenerarTokenRestablecimientoAsync(request.Email);

            if (token is not null)
            {
                // Envio del token por correo; si falla no se expone nada al cliente
                // (el token tambien viaja en la respuesta solo fuera de Production).
                try
                {
                    var email = new Email
                    {
                        To = request.Email,
                        Subject = "Restablecimiento de contrase&#241;a de AquaClinic",
                        Body = $"Hola,\n\nRecibimos una solicitud para restablecer tu contrase&#241;a de AquaClinic.\n\nUsa el siguiente codigo (v&#225;lido por un corto periodo):\n\n{token}\n\nSi no solicitaste este cambio, ignora este correo."
                    };

                    await _emailService.SendEmail(email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar el correo de restablecimiento a {Email}", request.Email);
                }
            }

            // Siempre aceptado: no se revela si el correo existe o no
            return new OlvidePasswordResponse(true, token);
        }
    }

    public class OlvidePasswordCommandValidator : AbstractValidator<OlvidePasswordCommand>
    {
        public OlvidePasswordCommandValidator()
        {
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .EmailAddress().WithMessage("{PropertyName} no tiene un formato v&#225;lido.");
        }
    }
}
