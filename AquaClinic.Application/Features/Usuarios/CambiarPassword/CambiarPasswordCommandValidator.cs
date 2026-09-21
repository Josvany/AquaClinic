using FluentValidation;

namespace AquaClinic.Application.Features.Usuarios.CambiarPassword
{
    public class CambiarPasswordCommandValidator : AbstractValidator<CambiarPasswordCommand>
    {
        public CambiarPasswordCommandValidator()
        {
            RuleFor(p => p.PasswordActual)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.");

            RuleFor(p => p.PasswordNueva)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MinimumLength(8).WithMessage("{PropertyName} debe tener al menos 8 caracteres.")
                .NotEqual(p => p.PasswordActual).WithMessage("La nueva contrase&#241;a debe ser diferente a la actual.");
        }
    }
}
