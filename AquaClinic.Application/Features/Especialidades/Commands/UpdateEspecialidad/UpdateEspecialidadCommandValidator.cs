using FluentValidation;

namespace AquaClinic.Application.Features.Especialidades.Commands.UpdateEspecialidad
{
    public class UpdateEspecialidadCommandValidator : AbstractValidator<UpdateEspecialidadCommand>
    {
        public UpdateEspecialidadCommandValidator()
        {
            RuleFor(e => e.EspecialidadId)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.");

            RuleFor(e => e.Nombre)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MaximumLength(100).WithMessage("{PropertyName} no debe exceder 100 caracteres.");
        }
    }
}