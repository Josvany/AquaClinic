using FluentValidation;

namespace AquaClinic.Application.Features.Usuarios.Registrarse
{
    public class RegistrarseCommandValidator : AbstractValidator<RegistrarseCommand>
    {
        public RegistrarseCommandValidator()
        {
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .EmailAddress().WithMessage("{PropertyName} no tiene un formato v&#225;lido.");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MinimumLength(8).WithMessage("{PropertyName} debe tener al menos 8 caracteres.");

            RuleFor(p => p.Nombre)
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Apellidos)
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Telefono)
                .MaximumLength(30).WithMessage("{PropertyName} no debe exceder 30 caracteres.");
        }
    }
}
