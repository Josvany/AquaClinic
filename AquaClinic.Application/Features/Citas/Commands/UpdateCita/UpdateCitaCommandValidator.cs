using FluentValidation;

namespace AquaClinic.Application.Features.Citas.Commands.UpdateCita
{
    public class UpdateCitaCommandValidator : AbstractValidator<UpdateCitaCommand>
    {
        public UpdateCitaCommandValidator()
        {
            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .NotNull()
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Precio)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .GreaterThan(0);
        }
    }
}
