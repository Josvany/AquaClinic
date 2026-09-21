using FluentValidation;

namespace AquaClinic.Application.Features.Servicios.Commands.UpdateServicio
{
    public class UpdateServicioCommandValidator : AbstractValidator<UpdateServicioCommand>
    {
        public UpdateServicioCommandValidator()
        {
            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Precio)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .GreaterThan(0).WithMessage("{PropertyName} debe ser un valor positivo.");

            RuleFor(p => p.DuracionMinutos)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .GreaterThan(0).WithMessage("{PropertyName} debe ser mayor que cero.");
        }
    }
}
