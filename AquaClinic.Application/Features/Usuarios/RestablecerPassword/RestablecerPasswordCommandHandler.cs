using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Application.Exceptions;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.RestablecerPassword
{
    public class RestablecerPasswordCommandHandler : IRequestHandler<RestablecerPasswordCommand>
    {
        private readonly IIdentityService _identityService;

        public RestablecerPasswordCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task Handle(RestablecerPasswordCommand request, CancellationToken cancellationToken)
        {
            var validator = new RestablecerPasswordCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            var resultado = await _identityService.RestablecerPasswordAsync(
                request.Email, request.Token, request.PasswordNueva);

            if (!resultado.Exito)
                throw new BadRequestException(string.Join(" ", resultado.Errores));
        }
    }
}
