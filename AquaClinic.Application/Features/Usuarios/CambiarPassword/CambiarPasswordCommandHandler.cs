using AquaClinic.Application.Contracts;
using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Application.Exceptions;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.CambiarPassword
{
    public class CambiarPasswordCommandHandler : IRequestHandler<CambiarPasswordCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly ILoggedInUserService _loggedInUserService;

        public CambiarPasswordCommandHandler(IIdentityService identityService, ILoggedInUserService loggedInUserService)
        {
            _identityService = identityService;
            _loggedInUserService = loggedInUserService;
        }

        public async Task Handle(CambiarPasswordCommand request, CancellationToken cancellationToken)
        {
            var validator = new CambiarPasswordCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            var resultado = await _identityService.CambiarPasswordAsync(
                _loggedInUserService.UserId, request.PasswordActual, request.PasswordNueva);

            if (!resultado.Exito)
                throw new BadRequestException(string.Join(" ", resultado.Errores));
        }
    }
}
