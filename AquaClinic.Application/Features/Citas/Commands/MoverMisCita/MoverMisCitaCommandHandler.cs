using AquaClinic.Application.Contracts;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Application.Features.Citas.Commands.MoverCita;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.MoverMisCita
{
    /// <summary>
    /// Reprogramacion de cita por parte del cliente. Verifica que la cita
    /// pertenezca al usuario autenticado y que todavia no haya pasado, y luego
    /// delega en MoverCitaCommandHandler para no duplicar las reglas de
    /// disponibilidad (horario de clinica, bloqueos y solapamientos).
    /// </summary>
    public class MoverMisCitaCommandHandler : IRequestHandler<MoverMisCitaCommand, Guid>
    {
        private readonly ICitaRepository _citaRepository;
        private readonly ILoggedInUserService _loggedInUserService;
        private readonly ISender _sender;

        public MoverMisCitaCommandHandler(
            ICitaRepository citaRepository,
            ILoggedInUserService loggedInUserService,
            ISender sender)
        {
            _citaRepository = citaRepository;
            _loggedInUserService = loggedInUserService;
            _sender = sender;
        }

        public async Task<Guid> Handle(MoverMisCitaCommand request, CancellationToken cancellationToken)
        {
            var usuarioId = _loggedInUserService.UserId;

            if (string.IsNullOrEmpty(usuarioId))
            {
                throw new BadRequestException("No se pudo identificar al usuario.");
            }

            var cita = await _citaRepository.GetByIdWithServiciosAsync(request.CitaId)
                ?? throw new NotFoundException(nameof(Cita), request.CitaId);

            // La cita debe pertenecer al usuario: un cliente nunca puede
            // reprogramar la cita de otra persona.
            if (!string.Equals(cita.UsuarioId, usuarioId, StringComparison.Ordinal))
            {
                throw new NotFoundException(nameof(Cita), request.CitaId);
            }

            if (cita.Estado is not (EstadoCita.Pendiente or EstadoCita.Confirmada))
            {
                throw new BadRequestException("Solo se pueden cambiar citas pendientes o confirmadas.");
            }

            // Cita ya atendida: no tiene sentido reprogramarla. La comparacion
            // se hace contra la hora de la clinica, no la de la maquina.
            var inicioLocalActual = CitaFechas.ALocal(cita.FechaInicio);
            if (inicioLocalActual <= AhoraClinica())
            {
                throw new BadRequestException("No puedes cambiar una cita que ya paso.");
            }

            // Reutiliza la misma logica de disponibilidad del uso interno
            // (horario de clinica, bloqueos y solapamientos).
            return await _sender.Send(new MoverCitaCommand
            {
                CitaId = request.CitaId,
                NuevoInicioLocal = request.NuevoInicioLocal,
                DuracionMinutos = request.DuracionMinutos
            }, cancellationToken);
        }

        /// <summary>
        /// Instante actual en la zona horaria de la clinica
        /// (America/Vancouver). Si el identificador de zona no existe en el
        /// sistema operativo se cae a la hora local.
        /// </summary>
        private static DateTime AhoraClinica()
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
}