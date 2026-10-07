using FluentValidation;
using AquaClinic.Domain.Enums;

namespace AquaClinic.Application.Features.Promociones.Commands.UpdatePromocion
{
    public class UpdatePromocionCommandValidator : AbstractValidator<UpdatePromocionCommand>
    {
        public UpdatePromocionCommandValidator()
        {
            RuleFor(p => p.PromocionId)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.");

            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("{PropertyName} es obligatorio.")
                .MaximumLength(100).WithMessage("{PropertyName} no debe exceder 100 caracteres.");

            RuleFor(p => p.Descripcion)
                .MaximumLength(500).WithMessage("{PropertyName} no debe exceder 500 caracteres.");

            RuleFor(p => p.TipoDescuento)
                .IsInEnum().WithMessage("{PropertyName} no es valido.");

            RuleFor(p => p.Valor)
                .GreaterThan(0).WithMessage("El valor del descuento debe ser mayor que cero.")
                .When(p => p.TipoDescuento == TipoDescuento.Porcentaje);

            RuleFor(p => p.Valor)
                .LessThanOrEqualTo(100).WithMessage("Un descuento en porcentaje no puede superar el 100%.")
                .When(p => p.TipoDescuento == TipoDescuento.Porcentaje);

            RuleFor(p => p.Valor)
                .LessThanOrEqualTo(10000).WithMessage("Un descuento fijo no puede superar los 10 000 CAD.")
                .When(p => p.TipoDescuento == TipoDescuento.Monto);

            RuleFor(p => p.VigenciaHasta)
                .GreaterThan(p => p.VigenciaDesde).WithMessage("La vigencia debe terminar despues de empezar.")
                .When(p => p.VigenciaDesde.HasValue && p.VigenciaHasta.HasValue);

            RuleFor(p => p.ServicioIds)
                .Must(lista => lista.Distinct().Count() <= 100)
                .WithMessage("No se pueden asociar mas de 100 servicios.");
        }
    }
}