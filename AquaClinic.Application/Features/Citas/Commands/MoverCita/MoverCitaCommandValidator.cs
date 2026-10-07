using FluentValidation;

namespace AquaClinic.Application.Features.Citas.Commands.MoverCita
{
    public class MoverCitaCommandValidator : AbstractValidator<MoverCitaCommand>
    {
        /// <summary>
        /// Dia actual en la zona de la clinica (America/Vancouver): la maquina
        /// puede estar en otra zona y el "hoy" no coincidiria con el del negocio.
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

        /// <summary>
        /// Hora actual en la zona de la clinica. Las fechas de las citas llegan
        /// en hora local de la clinica (no de la maquina): compararlas contra
        /// DateTime.Now rechazaria movimientos validos cuando el servidor esta
        /// en otra zona horaria.
        /// </summary>
        private static DateTime AhoraClinica
        {
            get
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
                    return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).DateTime;
                }
                catch (TimeZoneNotFoundException)
                {
                    return DateTime.Now;
                }
                catch (InvalidTimeZoneException)
                {
                    return DateTime.Now;
                }
            }
        }

        public MoverCitaCommandValidator()
        {
            RuleFor(c => c.NuevoInicioLocal)
                .Must(f => f != default).WithMessage("La nueva fecha de la cita es obligatoria.")
                .Must(f => f > AhoraClinica).WithMessage("No puedes mover la cita al pasado.")
                .Must(f => f.Date >= HoyClinica)
                    .WithMessage("No puedes mover la cita a fechas pasadas (dias o meses anteriores al actual).")
                .Must(f => f.Date <= HoyClinica.AddDays(60))
                    .WithMessage("Solo se puede agendar con un maximo de 60 dias de anticipacion.");

            RuleFor(c => c.DuracionMinutos)
                .GreaterThan(0).When(c => c.DuracionMinutos.HasValue)
                .WithMessage("La duracion debe ser mayor que cero.");
        }
    }
}
