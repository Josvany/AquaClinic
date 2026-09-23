using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Application.Features.Citas.Commands.CreateCita
{
    /// <summary>
    /// Crea la cita validando: existencia del servicio, horario de atencion de la
    /// clinica (America/Vancouver), bloqueos vigentes, solapamiento con citas
    /// activas y que la reserva suave seleccionada siga vigente.
    /// La confirmacion por correo se agregara mas adelante.
    /// </summary>
    public class CreateCitaCommandHandler : IRequestHandler<CreateCitaCommand, Guid>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IAgendaRepository _agendaRepository;
        private readonly ICitaRepository _citaRepository;
        private readonly IReservasSuavesStore _reservasSuaves;
        private readonly IAgregadorAgenda _agendador;
        private readonly ILogger<CreateCitaCommandHandler> _logger;

        public CreateCitaCommandHandler(
            IServicioRepository servicioRepository,
            IAgendaRepository agendaRepository,
            ICitaRepository citaRepository,
            IReservasSuavesStore reservasSuaves,
            IAgregadorAgenda agendador,
            ILogger<CreateCitaCommandHandler> logger)
        {
            _servicioRepository = servicioRepository;
            _agendaRepository = agendaRepository;
            _citaRepository = citaRepository;
            _reservasSuaves = reservasSuaves;
            _agendador = agendador;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCitaCommand request, CancellationToken cancellationToken)
        {
            var servicio = await _servicioRepository.GetByIdAsync(request.ServicioId);
            if (servicio is null || !servicio.Activo)
            {
                throw new NotFoundException(nameof(Servicio), request.ServicioId);
            }

            var inicioLocal = request.FechaInicioLocal;
            var duracion = request.DuracionMinutos ?? servicio.DuracionMinutos;
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

            // 3) Sin solapamiento con citas activas
            var citas = await _agendaRepository.GetCitasActivasEntreAsync(inicioUtc, finUtc);
            var solapa = citas.Any(c => c.FechaInicio < finUtc && c.FechaFin > inicioUtc);
            if (solapa)
            {
                throw new BadRequestException("Ese horario acaba de ser tomado. Elige otro disponible.");
            }

            // 4) Reserva suave: se libera al confirmar la cita (queda como Ocupado).
            _reservasSuaves.LiberarPorInicio(inicioUtc);

            var cita = new Cita
            {
                UsuarioId = string.IsNullOrEmpty(request.UsuarioId) ? null : request.UsuarioId,
                NombreInvitado = request.NombreInvitado,
                EmailInvitado = request.EmailInvitado,
                TelefonoInvitado = request.TelefonoInvitado,
                PersonalId = null, // asignacion de esteticista pendiente
                FechaInicio = inicioUtc,
                FechaFin = finUtc,
                Estado = EstadoCita.Pendiente,
                Notas = request.Notas
            };

            await _citaRepository.AddAsync(cita);

            await _citaRepository.AsignarServicioAsync(cita.CitaId, servicio.ServicioId);

            // Aviso en tiempo real: la hora pasa de "Reservando" a "Ocupado" para todos
            try
            {
                await _agendador.NotificarCitaCreadaAsync(new Features.Citas.ReservaSuaveDto(inicioUtc, finUtc));
            }
            catch (Exception ex)
            {
                // La difusion en tiempo real no debe impedir la reserva
                _logger.LogError("No se pudo notificar la cita {CitaId} en tiempo real: {Mensaje}",
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
                        $"El horario de atencion ese dia es de {horario.HoraApertura:hh\\:mm} a {horario.HoraCierre:hh\\:mm}.");
                }
            }
        }
    }
}
