using FluentValidation;
using AquaClinic.Application.Contracts.Persistence;

namespace AquaClinic.Application.Features.Servicios.Commands.CreateServicio
{
    public class CreateServicioCommandValidator : AbstractValidator<CreateServicioCommand>
    {
        private readonly IServicioRepository _servicioRepository;

        public CreateServicioCommandValidator(IServicioRepository servicioRepository)
        {
            _servicioRepository = servicioRepository;

            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Precio)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .GreaterThan(0).WithMessage("{PropertyName} debe ser un valor positivo.");

            RuleFor(p => p.DuracionMinutos)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .GreaterThan(0).WithMessage("{PropertyName} debe ser mayor que cero.");

            RuleFor(p => p.EspecialidadId)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.");

            RuleFor(e => e)
                .MustAsync(NombreUnico)
                .WithMessage("Ya existe un servicio con el mismo nombre.");
        }

        private async Task<bool> NombreUnico(CreateServicioCommand e, CancellationToken token)
        {
            return !await _servicioRepository.IsNombreUnico(e.Nombre);
        }
    }
}