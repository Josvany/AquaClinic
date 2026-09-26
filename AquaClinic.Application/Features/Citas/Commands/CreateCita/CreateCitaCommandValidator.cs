using FluentValidation;

namespace AquaClinic.Application.Features.Citas.Commands.CreateCita
{
    public class CreateCitaCommandValidator : AbstractValidator<CreateCitaCommand>
    {
        /// <summary>
        /// Dia actual en la zona de la clinica (America/Vancouver): la maquina puede
        /// estar en otra zona y el "hoy" no coincidiria con el del negocio.
        /// </summary>
        private static DateTime HoyClinica
        {
            get
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
                    return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).Date;
                }
                catch (TimeZoneNotFoundException)
                {
                    return DateTime.Today;
                }
                catch (InvalidTimeZoneException)
                {
                    return DateTime.Today;
                }
            }
        }

        public CreateCitaCommandValidator()
        {
            RuleFor(c => c.ServicioId)
                .NotEmpty().WithMessage("El servicio es obligatorio.");

            RuleFor(c => c.FechaInicioLocal)
                .Must(f => f != default).WithMessage("La fecha de la cita es obligatoria.")
                .Must(f => f > DateTime.Now).WithMessage("No puedes agendar una cita en el pasado.")
                .Must(f => f.Date >= HoyClinica)
                    .WithMessage("No puedes agendar en fechas pasadas (dias o meses anteriores al actual).")
                .Must(f => f.Date <= HoyClinica.AddDays(60))
                    .WithMessage("Solo se puede agendar con un maximo de 60 dias de anticipacion.");

            // Cliente invitado: datos de contacto obligatorios
            RuleFor(c => c)
                .Must(c => !string.IsNullOrEmpty(c.UsuarioId)
                           || (!string.IsNullOrWhiteSpace(c.NombreInvitado)
                               && !string.IsNullOrWhiteSpace(c.EmailInvitado)))
                .WithMessage("Para agendar como invitado indica tu nombre y correo.");

            RuleFor(c => c.EmailInvitado)
                .EmailAddress()
                .When(c => !string.IsNullOrWhiteSpace(c.EmailInvitado))
                .WithMessage("El correo del invitado no es valido.");

            RuleFor(c => c.Notas)
                .MaximumLength(500).WithMessage("Las notas no deben exceder 500 caracteres.");
        }
    }
}
