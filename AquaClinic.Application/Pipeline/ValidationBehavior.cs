using FluentValidation;
using MediatR;
using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using ValidationException = AquaClinic.Application.Exceptions.ValidationException;

namespace AquaClinic.Application.Pipeline
{
    /// <summary>
    /// Pipeline behavior de MediatR para validar comandos y queries mediante FluentValidation.
    /// Intercepta las solicitudes antes de llegar al handler correspondiente.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                // Preservar la compatibilidad con CreateEspecialidadCommandResponse (patrón Result)
                if (typeof(TResponse) == typeof(CreateEspecialidadCommandResponse))
                {
                    var response = new CreateEspecialidadCommandResponse
                    {
                        Success = false,
                        ValidationErrors = failures.Select(f => f.ErrorMessage).ToList()
                    };
                    return (TResponse)(object)response;
                }

                var result = new FluentValidation.Results.ValidationResult(failures);
                throw new ValidationException(result);
            }

            return await next();
        }
    }
}
