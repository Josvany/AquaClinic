using FluentValidation;
using AquaClinic.Application.Contracts.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaClinic.Application.Features.Citas.Commands.CreateCita
{
    public class CreateCitaCommandValidator : AbstractValidator<CreateCitaCommand>
    {
        private readonly ICitaRepository _citaRepository;
        public CreateCitaCommandValidator(ICitaRepository citaRepository)
        {
            _citaRepository = citaRepository;

            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .NotNull()
                .MaximumLength(50).WithMessage("{PropertyName} no debe exceder 50 caracteres.");

            RuleFor(p => p.Fecha)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .NotNull()
                .GreaterThan(DateTime.Now);

            RuleFor(e => e)
                .MustAsync(CitaNombreYFechaUnico)
                .WithMessage("Ya existe una cita con el mismo nombre y fecha.");

            RuleFor(p => p.Precio)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .GreaterThan(0);
        }

        private async Task<bool> CitaNombreYFechaUnico(CreateCitaCommand e, CancellationToken token)
        {
            return !await _citaRepository.IsCitaNombreYFechaUnico(e.Nombre, e.Fecha);
        }
    }
}
