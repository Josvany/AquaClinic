using FluentValidation;

namespace AquaClinic.Application.Features.Bloqueos.Commands.UpdateBloqueo
{
    public class UpdateBloqueoCommandValidator : AbstractValidator<UpdateBloqueoCommand>
    {
        public UpdateBloqueoCommandValidator()
        {
            RuleFor(p => p.BloqueoId)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.");

            RuleFor(p => p.Motivo)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MaximumLength(100).WithMessage("{PropertyName} no debe exceder 100 caracteres.");

            RuleFor(p => p.FechaInicio)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.");

            RuleFor(p => p.FechaFin)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .GreaterThan(p => p.FechaInicio).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");
        }
    }
}
