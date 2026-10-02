using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Application.Features.Citas;
using AquaClinic.Application.Features.Citas.Commands.CreateCita;
using AquaClinic.Application.Models.Mail;
using AquaClinic.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace AquaClinic.Application.UnitTests.Citas.Commands
{
    /// <summary>
    /// Verifica la regla "la clinica esta cerrada" al crear una cita: los bloqueos
    /// de toda la clinica (PersonalId = null) que se solapan con el rango de la
    /// cita rechazan la reserva, los rangos que solo se tocan en el borde se
    /// permiten y los bloqueos de una persona no cierran la clinica.
    /// El mock de IAgendaRepository emula el contrato del repositorio real:
    /// devuelve solo los bloqueos de clinica cuyo rango se solapa con el solicitado
    /// (la consulta en si se verifica en Persistence.IntegrationTests).
    /// </summary>
    public class CreateCitaBloqueosTests
    {
        private readonly List<Bloqueo> _bloqueosSembrados = new();
        private readonly Mock<IServicioRepository> _mockServicioRepository;
        private readonly Mock<IAgendaRepository> _mockAgendaRepository;
        private readonly Mock<ICitaRepository> _mockCitaRepository;
        private readonly Mock<IReservasSuavesStore> _mockReservasSuaves;
        private readonly Mock<IAgregadorAgenda> _mockAgendador;
        private readonly Mock<IEmailPendienteRepository> _mockEmailsPendientes;
        private readonly Mock<IEmailService> _mockEmailService;

        private readonly Guid _servicioId = Guid.NewGuid();

        /// <summary>Lunes futuro: los tests de agendado no pueden usar fechas pasadas.</summary>
        private readonly DateTime _lunes;

        public CreateCitaBloqueosTests()
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

            // Lunes abierto de 9 a 18: el validador de horario deja pasar la cita.
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
            _mockAgendaRepository
                .Setup(r => r.GetBloqueosClinicaEntreAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((DateTimeOffset inicio, DateTimeOffset fin) => _bloqueosSembrados
                    .Where(b => b.PersonalId == null && b.FechaInicio < fin && b.FechaFin > inicio)
                    .ToList());
            _mockAgendaRepository.Setup(r => r.GetCitasActivasEntreAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Cita>());

            // Margen de 2 dias: siempre futuro en la zona de la clinica sin importar
            // la zona horaria de la maquina que ejecuta los tests.
            _lunes = DateTime.Today.AddDays(2);
            while (_lunes.DayOfWeek != DayOfWeek.Monday)
            {
                _lunes = _lunes.AddDays(1);
            }
        }

        private void SembrarBloqueo(Bloqueo bloqueo)
        {
            _bloqueosSembrados.Add(bloqueo);
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

        private CreateCitaCommand CrearComando(int horaInicio = 10, int? duracionMinutos = null) => new()
        {
            ServicioId = _servicioId,
            FechaInicioLocal = _lunes.AddHours(horaInicio),
            DuracionMinutos = duracionMinutos,
            EmailInvitado = "cliente@example.com",
            NombreInvitado = "Ana"
        };

        [Fact]
        public async Task Handle_BloqueoDeClinicaQueContieneLaCita_RementaYNoCreaCita()
        {
            // Arrange: bloqueo de clinica de 9 a 17; la cita pide 10 a 11.
            SembrarBloqueo(new Bloqueo
            {
                Motivo = "Capacitación del equipo",
                FechaInicio = CitaFechas.AUtc(_lunes.AddHours(9)),
                FechaFin = CitaFechas.AUtc(_lunes.AddHours(17)),
                PersonalId = null
            });

            // Captura del rango que el handler envia al repositorio: debe ser UTC.
            var iniciosConsultados = new List<DateTimeOffset>();
            var finesConsultados = new List<DateTimeOffset>();
            _mockAgendaRepository
                .Setup(r => r.GetBloqueosClinicaEntreAsync(Capture.In(iniciosConsultados), Capture.In(finesConsultados)))
                .ReturnsAsync((DateTimeOffset inicio, DateTimeOffset fin) => _bloqueosSembrados
                    .Where(b => b.PersonalId == null && b.FechaInicio < fin && b.FechaFin > inicio)
                    .ToList());

            // Act
            var excepcion = await Should.ThrowAsync<BadRequestException>(() =>
                CrearHandler().Handle(CrearComando(), CancellationToken.None));

            // Assert: el mensaje incluye el motivo del bloqueo y no se crea la cita
            excepcion.Message.ShouldContain("Capacitación del equipo");
            _mockCitaRepository.Verify(r => r.AddAsync(It.IsAny<Cita>()), Times.Never);

            // Assert: la consulta de bloqueos se hace con el rango convertido a UTC
            iniciosConsultados.Single().ShouldBe(CitaFechas.AUtc(_lunes.AddHours(10)));
            finesConsultados.Single().ShouldBe(CitaFechas.AUtc(_lunes.AddHours(11)));
        }

        [Fact]
        public async Task Handle_BloqueoQueEmpiezaDuranteLaCita_Rementa()
        {
            // Arrange: cita larga de 9 a 12 (180 min) y bloqueo desde las 10:30:
            // el solape parcial al final de la cita tambien cierra la clinica.
            SembrarBloqueo(new Bloqueo
            {
                Motivo = "Cierre por mantenimiento",
                FechaInicio = CitaFechas.AUtc(_lunes.AddHours(10.5)),
                FechaFin = CitaFechas.AUtc(_lunes.AddHours(13)),
                PersonalId = null
            });

            // Act
            var excepcion = await Should.ThrowAsync<BadRequestException>(() =>
                CrearHandler().Handle(CrearComando(horaInicio: 9, duracionMinutos: 180), CancellationToken.None));

            // Assert
            excepcion.Message.ShouldContain("mantenimiento");
            _mockCitaRepository.Verify(r => r.AddAsync(It.IsAny<Cita>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BloqueoTerminaJustoAlIniciarLaCita_PermiteReservar()
        {
            // Arrange: bloqueo de 8 a 10 y cita de 10 a 11: el solape es estricto,
            // los rangos que solo se tocan en el borde NO se consideran solapados.
            SembrarBloqueo(new Bloqueo
            {
                Motivo = "Capacitación del equipo",
                FechaInicio = CitaFechas.AUtc(_lunes.AddHours(8)),
                FechaFin = CitaFechas.AUtc(_lunes.AddHours(10)),
                PersonalId = null
            });

            // Act
            var citaId = await CrearHandler().Handle(CrearComando(), CancellationToken.None);

            // Assert: la cita se crea a pesar del bloqueo adyacente
            citaId.ShouldNotBe(Guid.Empty);
            _mockCitaRepository.Verify(r => r.AddAsync(It.IsAny<Cita>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BloqueoDeUnaPersona_NoCierraLaClinica()
        {
            // Arrange: alcance actual del sistema: solo los bloqueos de toda la
            // clinica (PersonalId = null) cierran la agenda; el repositorio no
            // devuelve los de una persona y la cita se crea normalmente.
            SembrarBloqueo(new Bloqueo
            {
                Motivo = "Vacaciones de la esteticista",
                FechaInicio = CitaFechas.AUtc(_lunes.AddHours(10)),
                FechaFin = CitaFechas.AUtc(_lunes.AddHours(11)),
                PersonalId = Guid.NewGuid()
            });

            // Act
            var citaId = await CrearHandler().Handle(CrearComando(), CancellationToken.None);

            // Assert
            citaId.ShouldNotBe(Guid.Empty);
            _mockCitaRepository.Verify(r => r.AddAsync(It.IsAny<Cita>()), Times.Once);
        }
    }
}
