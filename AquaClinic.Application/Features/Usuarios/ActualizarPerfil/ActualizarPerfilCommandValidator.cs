using FluentValidation;

namespace AquaClinic.Application.Features.Usuarios.ActualizarPerfil
{
    public class ActualizarPerfilCommandValidator : AbstractValidator<ActualizarPerfilCommand>
    {
        public ActualizarPerfilCommandValidator()
        {
            RuleFor(p => p.Nombre)
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Apellidos)
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Telefono)
                .MaximumLength(30).WithMessage("{PropertyName} no debe exceder 30 caracteres.");
        }
    }
}
