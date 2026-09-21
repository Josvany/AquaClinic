using AquaClinic.Application.Contracts;
using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Application.Exceptions;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.ActualizarPerfil
{
    public class ActualizarPerfilCommandHandler : IRequestHandler<ActualizarPerfilCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly ILoggedInUserService _loggedInUserService;

        public ActualizarPerfilCommandHandler(IIdentityService identityService, ILoggedInUserService loggedInUserService)
        {
            _identityService = identityService;
            _loggedInUserService = loggedInUserService;
        }

        public async Task Handle(ActualizarPerfilCommand request, CancellationToken cancellationToken)
        {
            var validator = new ActualizarPerfilCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            var resultado = await _identityService.ActualizarPerfilAsync(
                _loggedInUserService.UserId, request.Nombre, request.Apellidos, request.Telefono);

            if (!resultado.Exito)
                throw new BadRequestException(string.Join(" ", resultado.Errores));
        }
    }
}
