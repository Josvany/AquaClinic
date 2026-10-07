using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Citas.Commands.CreateCita;
using AquaClinic.Application.Models.Mail;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace AquaClinic.Application.UnitTests.Citas.Commands
{
    public class CreateCitaTests
    {
        private readonly Mock<IServicioRepository> _mockServicioRepository;
        private readonly Mock<IAgendaRepository> _mockAgendaRepository;
        private readonly Mock<ICitaRepository> _mockCitaRepository;
        private readonly Mock<IReservasSuavesStore> _mockReservasSuaves;
        private readonly Mock<IAgregadorAgenda> _mockAgendador;
        private readonly Mock<IEmailPendienteRepository> _mockEmailsPendientes;
        private readonly Mock<IEmailService> _mockEmailService;

        private readonly Guid _servicioId = Guid.NewGuid();

        public CreateCitaTests()
        {
            _mockServicioRepository = new Mock<IServicioRepository>();
            _mockAgendaRepository = new Mock<IAgendaRepository>();
            _mockCitaRepository = new Mock<ICitaRepository>();
            _mockReservasSuaves = new Mock<IReservasSuavesStore>();
            _mockAgendador = new Mock<IAgregadorAgenda>();
            _mockEmailsPendientes = new Mock<IEmailPendienteRepository>();
            _mockEmailService = new Mock<IEmailService>();

            _mockServicioRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Servicio
                {
                    ServicioId = _servicioId,
                    Nombre = "Limpieza facial",
                    DuracionMinutos = 60,
                    Precio = 100m,
                    Activo = true
                });

            _mockAgendaRepository.Setup(r => r.GetHorariosAsync())
                .ReturnsAsync(new List<HorarioClinica>());
            _mockAgendaRepository.Setup(r => r.GetBloqueosClinicaEntreAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Bloqueo>());
            _mockAgendaRepository.Setup(r => r.GetCitasActivasEntreAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Cita>());
        }

        private CreateCitaCommandHandler CrearHandler()
        {
            return new CreateCitaCommandHandler(
                _mockServicioRepository.Object,
                _mockAgendaRepository.Object,
                _mockCitaRepository.Object,
                _mockReservasSuaves.Object,
                _mockAgendador.Object,
                _mockEmailsPendientes.Object,
                _mockEmailService.Object,
                NullLogger<CreateCitaCommandHandler>.Instance);
        }

        /// <summary>Proximo lunes 9 AM en la zona de la clinica (America/Vancouver):
        /// siempre estrictamente futuro para no chocar con la validacion de fechas
        /// pasadas del CreateCitaCommandValidator (una fecha fija caduca con el tiempo).
        /// </summary>
        private static DateTime ProximoLunes9Am()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
                var hoy = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).Date;
                var dias = ((int)DayOfWeek.Monday - (int)hoy.DayOfWeek + 7) % 7;
                if (dias == 0) dias = 7;
                return hoy.AddDays(dias).AddHours(9);
            }
            catch (TimeZoneNotFoundException)
            {
                var hoy = DateTime.Today;
                var dias = ((int)DayOfWeek.Monday - (int)hoy.DayOfWeek + 7) % 7;
                if (dias == 0) dias = 7;
                return hoy.AddDays(dias).AddHours(9);
            }
            catch (InvalidTimeZoneException)
            {
                var hoy = DateTime.Today;
                var dias = ((int)DayOfWeek.Monday - (int)hoy.DayOfWeek + 7) % 7;
                if (dias == 0) dias = 7;
                return hoy.AddDays(dias).AddHours(9);
            }
        }

        private CreateCitaCommand CrearComando() => new()
        {
            ServicioId = _servicioId,
            // Proximo lunes 9 AM (hora de la clinica): el mock de horarios del test
            // especifico devuelve el lunes abierto; la lista vacia por defecto hace
            // que ValidarHorarioClinica no encuentre restricciones. Ver ProximoLunes9Am.
            FechaInicioLocal = ProximoLunes9Am(),
            EmailInvitado = "cliente@example.com",
            NombreInvitado = "Ana"
        };

        [Fact]
        public async Task Handle_CitaValida_RegistraEmailPendienteYEnvia()
        {
            // Arrange: horario abierto el lunes del comando
            _mockAgendaRepository.Setup(r => r.GetHorariosAsync())
                .ReturnsAsync(new List<HorarioClinica>
                {
                    new()
                    {
                        DiaSemana = DayOfWeek.Monday,
                        HoraApertura = new TimeOnly(9, 0),
                        HoraCierre = new TimeOnly(18, 0),
                        Cerrado = false
                    }
                });
            _mockEmailService.Setup(s => s.SendEmail(It.IsAny<Email>())).ReturnsAsync(true);

            // Act
            await CrearHandler().Handle(CrearComando(), CancellationToken.None);

            // Assert: se grabo el correo en el outbox. La captura guarda la
            // referencia y el handler la muta al confirmar el envio, asi que
            // aqui solo se validan campos inmutables (destinatario y cita).
            var encolados = new List<EmailPendiente>();
            _mockEmailsPendientes.Verify(r => r.AddAsync(Capture.In(encolados)), Times.Once);
            encolados.ShouldHaveSingleItem();
            var encolado = encolados[0];
            encolado.Destinatario.ShouldBe("cliente@example.com");
            encolado.CitaId.ShouldNotBeNull();

            // Assert: intento inmediato y confirmacion de envio
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<Email>()), Times.Once);
            _mockEmailsPendientes.Verify(r => r.UpdateAsync(It.Is<EmailPendiente>(e =>
                e.Estado == EstadoEmail.Enviado &&
                e.Intentos == 1 &&
                e.EnviadoUtc != null)), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailServiceFalla_LaCitaSeCreaYQuedaPendiente()
        {
            // Arrange
            _mockAgendaRepository.Setup(r => r.GetHorariosAsync())
                .ReturnsAsync(new List<HorarioClinica>
                {
                    new()
                    {
                        DiaSemana = DayOfWeek.Monday,
                        HoraApertura = new TimeOnly(9, 0),
                        HoraCierre = new TimeOnly(18, 0),
                        Cerrado = false
                    }
                });
            _mockEmailService.Setup(s => s.SendEmail(It.IsAny<Email>())).ReturnsAsync(false);

            // Act: NO debe lanzar excepcion aunque el correo falle
            var citaId = await CrearHandler().Handle(CrearComando(), CancellationToken.None);

            // Assert: la cita se creo
            citaId.ShouldNotBe(Guid.Empty);
            _mockCitaRepository.Verify(r => r.AddAsync(It.IsAny<Cita>()), Times.Once);

            // Assert: el correo queda Pendiente con 1 intento para reintento
            _mockEmailsPendientes.Verify(r => r.UpdateAsync(It.Is<EmailPendiente>(e =>
                e.Estado == EstadoEmail.Pendiente &&
                e.Intentos == 1 &&
                e.UltimoIntentoUtc != null &&
                e.UltimoError != null)), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailServiceLanzaExcepcion_LaCitaSeCreaIgualmente()
        {
            // Arrange
            _mockAgendaRepository.Setup(r => r.GetHorariosAsync())
                .ReturnsAsync(new List<HorarioClinica>
                {
                    new()
                    {
                        DiaSemana = DayOfWeek.Monday,
                        HoraApertura = new TimeOnly(9, 0),
                        HoraCierre = new TimeOnly(18, 0),
                        Cerrado = false
                    }
                });
            _mockEmailService.Setup(s => s.SendEmail(It.IsAny<Email>()))
                .ThrowsAsync(new HttpRequestException("Mailjet no disponible"));

            // Act
            var citaId = await CrearHandler().Handle(CrearComando(), CancellationToken.None);

            // Assert: la excepcion no se propaga y la cita existe
            citaId.ShouldNotBe(Guid.Empty);
            _mockCitaRepository.Verify(r => r.AddAsync(It.IsAny<Cita>()), Times.Once);
            _mockEmailsPendientes.Verify(r => r.UpdateAsync(It.Is<EmailPendiente>(e =>
                e.Estado == EstadoEmail.Pendiente &&
                e.Intentos == 1)), Times.Once);
        }

        [Fact]
        public async Task Handle_UsuarioRegistradoSinEmailInvitado_NoEncolaCorreo()
        {
            // Arrange: un usuario registrado no deja EmailInvitado; la validacion
            // de contacto de invitado no aplica y hoy no hay correo de reserva.
            _mockAgendaRepository.Setup(r => r.GetHorariosAsync())
                .ReturnsAsync(new List<HorarioClinica>
                {
                    new()
                    {
                        DiaSemana = DayOfWeek.Monday,
                        HoraApertura = new TimeOnly(9, 0),
                        HoraCierre = new TimeOnly(18, 0),
                        Cerrado = false
                    }
                });
            var comando = CrearComando();
            comando.EmailInvitado = null;
            comando.UsuarioId = "user-123";

            // Act
            await CrearHandler().Handle(comando, CancellationToken.None);

            // Assert: sin destinatario no hay outbox ni envio, pero la cita se crea
            _mockCitaRepository.Verify(r => r.AddAsync(It.IsAny<Cita>()), Times.Once);
            _mockEmailsPendientes.Verify(r => r.AddAsync(It.IsAny<EmailPendiente>()), Times.Never);
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<Email>()), Times.Never);
        }
    }
}
