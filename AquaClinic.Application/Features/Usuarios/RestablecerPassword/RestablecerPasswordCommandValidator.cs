using FluentValidation;

namespace AquaClinic.Application.Features.Usuarios.RestablecerPassword
{
    public class RestablecerPasswordCommandValidator : AbstractValidator<RestablecerPasswordCommand>
    {
        public RestablecerPasswordCommandValidator()
        {
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .EmailAddress().WithMessage("{PropertyName} no tiene un formato v&#225;lido.");

            RuleFor(p => p.Token)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.");

            RuleFor(p => p.PasswordNueva)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MinimumLength(8).WithMessage("{PropertyName} debe tener al menos 8 caracteres.");
        }
    }
}
