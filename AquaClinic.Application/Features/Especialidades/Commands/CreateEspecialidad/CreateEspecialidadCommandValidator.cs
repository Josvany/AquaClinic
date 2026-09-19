using FluentValidation;

namespace AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad
{
    public class CreateEspecialidadCommandValidator: AbstractValidator<CreateEspecialidadCommand>
    {
        public CreateEspecialidadCommandValidator()
        {
            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .NotNull()
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");
        }
    }
}
