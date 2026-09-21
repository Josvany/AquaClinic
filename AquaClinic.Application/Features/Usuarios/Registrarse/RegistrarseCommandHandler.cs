using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Application.Exceptions;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.Registrarse
{
    /// <summary>
    /// Registra un cliente nuevo con rol Cliente. Falla con BadRequest si el
    /// correo ya est&#225; registrado o la contrase&#241;a no cumple las reglas de Identity.
    /// </summary>
    public class RegistrarseCommandHandler : IRequestHandler<RegistrarseCommand, string>
    {
        private readonly IIdentityService _identityService;

        public RegistrarseCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<string> Handle(RegistrarseCommand request, CancellationToken cancellationToken)
        {
            var validator = new RegistrarseCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
                throw new ValidationException(validationResult);

            var resultado = await _identityService.RegistrarAsync(
                request.Email, request.Password, request.Nombre, request.Apellidos, request.Telefono);

            if (!resultado.Exito || resultado.UsuarioId is null)
                throw new BadRequestException(string.Join(" ", resultado.Errores));

            return resultado.UsuarioId;
        }
    }
}
