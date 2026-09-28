using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Application.Models.Mail;
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
    /// La confirmacion por correo se registra en la cola de salida (outbox) y se
    /// intenta de inmediato; si Mailjet falla, queda Pendiente para reintento y
    /// la cita se crea igualmente.
    /// </summary>
    public class CreateCitaCommandHandler : IRequestHandler<CreateCitaCommand, Guid>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IAgendaRepository _agendaRepository;
        private readonly ICitaRepository _citaRepository;
        private readonly IReservasSuavesStore _reservasSuaves;
        private readonly IAgregadorAgenda _agendador;
        private readonly IEmailPendienteRepository _emailsPendientes;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateCitaCommandHandler> _logger;

        public CreateCitaCommandHandler(
            IServicioRepository servicioRepository,
            IAgendaRepository agendaRepository,
            ICitaRepository citaRepository,
            IReservasSuavesStore reservasSuaves,
            IAgregadorAgenda agendador,
            IEmailPendienteRepository emailsPendientes,
            IEmailService emailService,
            ILogger<CreateCitaCommandHandler> logger)
        {
            _servicioRepository = servicioRepository;
            _agendaRepository = agendaRepository;
            _citaRepository = citaRepository;
            _reservasSuaves = reservasSuaves;
            _agendador = agendador;
            _emailsPendientes = emailsPendientes;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCitaCommand request, CancellationToken cancellationToken)
        {
            // Validaciones de entrada (fecha no pasada, contacto del invitado, ...)
            var validator = new CreateCitaCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.Errors.Count > 0)
            {
                throw new ValidationException(validationResult);
            }

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
                Estado = EstadoCita.Confirmada,
                Notas = request.Notas
            };

            await _citaRepository.AddAsync(cita);

            await _citaRepository.AsignarServicioAsync(cita.CitaId, servicio.ServicioId);

            // Confirmacion por correo: se persiste en la cola ANTES de intentar
            // el envio; un fallo de Mailjet queda registrado para reintento sin
            // afectar la cita.
            await EncolarConfirmacionAsync(cita, servicio.Nombre, inicioLocal, duracion);

            // Aviso en tiempo real: la hora pasa de "Reservando" a "Ocupado" para todos
            try
            {
                await _agendador.NotificarCitaCreadaAsync(new ReservaSuaveDto(inicioUtc, finUtc));
            }
            catch (Exception ex)
            {
                // La difusion en tiempo real no debe impedir la reserva
                _logger.LogError("No se pudo notificar la cita {CitaId} en tiempo real: {Mensaje}",
                    cita.CitaId, ex.Message);
            }

            return cita.CitaId;
        }

        private async Task EncolarConfirmacionAsync(Cita cita, string servicioNombre, DateTime inicioLocal, int duracion)
        {
            // Un correo sin destinatario no se encola (la cita sigue siendo valida)
            var destinatario = cita.EmailInvitado;
            if (string.IsNullOrWhiteSpace(destinatario))
            {
                _logger.LogWarning(
                    "Cita {CitaId} sin correo de contacto: no se encolara confirmacion",
                    cita.CitaId);
                return;
            }

            var finLocal = inicioLocal.AddMinutes(duracion);
            var email = new EmailPendiente
            {
                CitaId = cita.CitaId,
                Destinatario = destinatario,
                Asunto = "Confirmacion de tu cita - Aqua Skin & Glow Clinic",
                Cuerpo = $"Hola {(cita.NombreInvitado ?? "cliente")},\n\n" +
                    $"Tu cita quedo agendada:\n\n" +
                    $"Servicio: {servicioNombre}\n" +
                    $"Fecha: {inicioLocal:dddd dd 'de' MMMM 'de' yyyy}\n" +
                    $"Hora: {inicioLocal:hh:mm tt} - {finLocal:hh:mm tt} (hora de Vancouver)\n\n" +
                    $"Si necesitas reprogramar o cancelar, contactanos respondiendo este correo.\n\n" +
                    "Aqua Skin & Glow Clinic\nLangley, BC",
                Estado = EstadoEmail.Pendiente
            };

            await _emailsPendientes.AddAsync(email);

            // Intento inmediato; si falla, el servicio de reintentos lo retomara
            try
            {
                var enviado = await _emailService.SendEmail(new Email
                {
                    To = email.Destinatario,
                    Subject = email.Asunto,
                    Body = email.Cuerpo
                });

                email.Intentos = 1;
                email.UltimoIntentoUtc = DateTime.UtcNow;

                if (enviado)
                {
                    email.Estado = EstadoEmail.Enviado;
                    email.EnviadoUtc = DateTime.UtcNow;
                }
                else
                {
                    email.UltimoError = "El proveedor de correo rechazo el envio (intento inmediato).";
                }

                await _emailsPendientes.UpdateAsync(email);
            }
            catch (Exception ex)
            {
                // La cita ya esta creada: el fallo del correo no debe propagarse.
                email.Intentos = 1;
                email.UltimoIntentoUtc = DateTime.UtcNow;
                email.UltimoError = ex.Message;
                await _emailsPendientes.UpdateAsync(email);

                _logger.LogError(ex,
                    "Fallo el envio inmediato de la confirmacion de la cita {CitaId}; quedo en cola para reintento",
                    cita.CitaId);
            }
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