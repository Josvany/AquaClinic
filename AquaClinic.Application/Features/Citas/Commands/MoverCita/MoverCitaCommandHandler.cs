using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AquaClinic.Application.Features.Citas.Commands.MoverCita
{
    /// <summary>
    /// Mueve la cita validando las mismas reglas de disponibilidad que la
    /// creacion (horario de atencion de la clinica, bloqueos vigentes y
    /// solapamiento con otras citas activas, excluyendo la propia cita).
    /// Solo se pueden mover citas pendientes o confirmadas.
    /// </summary>
    public class MoverCitaCommandHandler : IRequestHandler<MoverCitaCommand, Guid>
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IAgendaRepository _agendaRepository;
        private readonly IReservasSuavesStore _reservasSuaves;
        private readonly IAgregadorAgenda _agendador;
        private readonly ILogger<MoverCitaCommandHandler> _logger;

        public MoverCitaCommandHandler(
            ICitaRepository citaRepository,
            IAgendaRepository agendaRepository,
            IReservasSuavesStore reservasSuaves,
            IAgregadorAgenda agendador,
            ILogger<MoverCitaCommandHandler> logger)
        {
            _citaRepository = citaRepository;
            _agendaRepository = agendaRepository;
            _reservasSuaves = reservasSuaves;
            _agendador = agendador;
            _logger = logger;
        }

        public async Task<Guid> Handle(MoverCitaCommand request, CancellationToken cancellationToken)
        {
            var validator = new MoverCitaCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
            {
                throw new ValidationException(validationResult);
            }

            var cita = await _citaRepository.GetByIdWithServiciosAsync(request.CitaId)
                ?? throw new NotFoundException(nameof(Cita), request.CitaId);

            if (cita.Estado is not (EstadoCita.Pendiente or EstadoCita.Confirmada))
            {
                throw new BadRequestException("Solo se pueden mover citas pendientes o confirmadas.");
            }

            var duracion = request.DuracionMinutos
                ?? cita.CitasServicios?.FirstOrDefault()?.Servicio?.DuracionMinutos
                ?? 60;

            var inicioLocal = request.NuevoInicioLocal;
            var finLocal = inicioLocal.AddMinutes(duracion);

            var inicioUtc = CitaFechas.AUtc(inicioLocal);
            var finUtc = CitaFechas.AUtc(finLocal);

            // 1) La clinica debe estar abierta todo el intervalo (hora local)
            await ValidarHorarioClinicaAsync(inicioLocal, finLocal);

            // 2) Sin bloqueos de clinica que se solapen
            var bloqueos = await _agendaRepository.GetBloqueosClinicaEntreAsync(inicioUtc, finUtc);
            if (bloqueos.Count > 0)
            {
                throw new BadRequestException(
                    $"La clinica esta cerrada en ese horario ({bloqueos[0].Motivo}). Elige otro horario.");
            }

            // 3) Sin solapamiento con otras citas activas (la propia cita queda excluida)
            var citas = await _agendaRepository.GetCitasActivasEntreAsync(inicioUtc, finUtc);
            var solapa = citas.Any(c => c.CitaId != cita.CitaId && c.FechaInicio < finUtc && c.FechaFin > inicioUtc);
            if (solapa)
            {
                throw new BadRequestException("Ese horario acaba de ser tomado. Elige otro disponible.");
            }

            cita.FechaInicio = inicioUtc;
            cita.FechaFin = finUtc;

            // Reserva suave: se libera al confirmar el movimiento (el
            // nuevo slot queda como Ocupado).
            _reservasSuaves.LiberarPorInicio(inicioUtc);

            await _citaRepository.UpdateAsync(cita);

            // Aviso en tiempo real: el nuevo slot pasa a "Ocupado" para todos.
            // El slot anterior queda libre al dejar de haber una cita ahi.
            try
            {
                await _agendador.NotificarCitaCreadaAsync(new ReservaSuaveDto(inicioUtc, finUtc));
            }
            catch (Exception ex)
            {
                // La difusion en tiempo real no debe impedir el movimiento
                _logger.LogError("No se pudo notificar el movimiento de la cita {CitaId} en tiempo real: {Mensaje}",
                    cita.CitaId, ex.Message);
            }

            return cita.CitaId;
        }

        private async Task ValidarHorarioClinicaAsync(DateTime inicioLocal, DateTime finLocal)
        {
            var horarios = await _agendaRepository.GetHorariosAsync();

            for (var dia = inicioLocal.Date; dia < finLocal; dia = dia.AddDays(1))
            {
                var horario = horarios.FirstOrDefault(h => h.DiaSemana == dia.DayOfWeek);
                if (horario is null || horario.Cerrado)
                {
                    throw new BadRequestException("La clinica esta cerrada el dia seleccionado.");
                }

                var apertura = dia.Add(horario.HoraApertura.ToTimeSpan());
                var cierre = dia.Add(horario.HoraCierre.ToTimeSpan());

                if (inicioLocal < apertura || finLocal > cierre)
                {
                    throw new BadRequestException(
                        $"El horario de atencion ese dia es de {horario.HoraApertura:HH\\:mm} a {horario.HoraCierre:HH\\:mm}.");
                }
            }
        }
    }
}
