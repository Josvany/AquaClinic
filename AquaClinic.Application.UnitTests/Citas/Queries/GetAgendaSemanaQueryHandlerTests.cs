using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Citas;
using AquaClinic.Application.Features.Citas.Queries.GetAgendaSemana;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using Moq;
using Shouldly;

namespace AquaClinic.Application.UnitTests.Citas.Queries
{
    /// <summary>
    /// Verifica la agenda semanal publica: citas activas como "Ocupado", reservas
    /// suaves como "Reservando" y bloqueos de clinica como "Bloqueado" (cierres no
    /// reservables). Los bloqueos de una persona no cierran la clinica y no salen.
    /// El mock del repositorio emula su contrato: solo bloqueos de clinica
    /// (PersonalId = null) cuyo rango se solapa con el solicitado, en UTC.
    /// </summary>
    public class GetAgendaSemanaQueryHandlerTests
    {
        private readonly Mock<ICitaRepository> _mockCitaRepository;
        private readonly Mock<IAgendaRepository> _mockAgendaRepository;
        private readonly Mock<IReservasSuavesStore> _mockReservasSuaves;

        /// <summary>Semana fija: del lunes 5-oct-2026 al lunes 12-oct-2026 (local).</summary>
        private static readonly DateTime SemanaInicio = new(2026, 10, 5);
        private static readonly DateTime SemanaFin = new(2026, 10, 12);

        public GetAgendaSemanaQueryHandlerTests()
        {
            _mockCitaRepository = new Mock<ICitaRepository>();
            _mockAgendaRepository = new Mock<IAgendaRepository>();
            _mockReservasSuaves = new Mock<IReservasSuavesStore>();

            _mockCitaRepository.Setup(r => r.GetCitasWithServiciosAsync())
                .ReturnsAsync(new List<Cita>());
            _mockReservasSuaves.Setup(r => r.ObtenerVigentes())
                .Returns(new List<ReservaSuaveDto>());
            _mockAgendaRepository
                .Setup(r => r.GetBloqueosClinicaEntreAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((DateTimeOffset inicio, DateTimeOffset fin) => new List<Bloqueo>
                {
                    // Dentro de la semana, de toda la clinica: si sale en la agenda
                    new()
                    {
                        Motivo = "Capacitación del personal",
                        FechaInicio = CitaFechas.AUtc(new DateTime(2026, 10, 7, 9, 0, 0)),
                        FechaFin = CitaFechas.AUtc(new DateTime(2026, 10, 7, 13, 0, 0)),
                        PersonalId = null
                    },
                    // Bloqueo de una persona: el repositorio NO lo devuelve
                    new()
                    {
                        Motivo = "Vacaciones de la esteticista",
                        FechaInicio = CitaFechas.AUtc(new DateTime(2026, 10, 8, 9, 0, 0)),
                        FechaFin = CitaFechas.AUtc(new DateTime(2026, 10, 8, 17, 0, 0)),
                        PersonalId = Guid.NewGuid()
                    },
                    // De la clinica pero en otra semana: no solapa, no sale
                    new()
                    {
                        Motivo = "Día de Acción de Gracias (Canadá)",
                        FechaInicio = CitaFechas.AUtc(new DateTime(2026, 10, 12, 9, 0, 0)),
                        FechaFin = CitaFechas.AUtc(new DateTime(2026, 10, 12, 18, 0, 0)),
                        PersonalId = null
                    }
                }
                .Where(b => b.PersonalId == null && b.FechaInicio < fin && b.FechaFin > inicio)
                .ToList());
        }

        private GetAgendaSemanaQueryHandler CrearHandler() =>
            new(_mockCitaRepository.Object, _mockAgendaRepository.Object, _mockReservasSuaves.Object);

        private static Cita CrearCita(EstadoCita estado, DateTime inicioLocal, DateTime finLocal) => new()
        {
            NombreInvitado = "Ana Torres",
            EmailInvitado = "ana.torres@example.com",
            FechaInicio = CitaFechas.AUtc(inicioLocal),
            FechaFin = CitaFechas.AUtc(finLocal),
            Estado = estado
        };

        [Fact]
        public async Task Handle_DevuelveCitasReservasYBloqueosDeClinicaConSuEstado()
        {
            // Arrange: citas dentro (confirmada y cancelada) y una de otra semana
            _mockCitaRepository.Setup(r => r.GetCitasWithServiciosAsync())
                .ReturnsAsync(new List<Cita>
                {
                    CrearCita(EstadoCita.Confirmada, new DateTime(2026, 10, 6, 11, 0, 0), new DateTime(2026, 10, 6, 12, 0, 0)),
                    CrearCita(EstadoCita.Cancelada, new DateTime(2026, 10, 7, 10, 0, 0), new DateTime(2026, 10, 7, 11, 0, 0)),
                    CrearCita(EstadoCita.Confirmada, new DateTime(2026, 10, 15, 9, 0, 0), new DateTime(2026, 10, 15, 10, 0, 0))
                });

            _mockAgendaRepository.Setup(r => r.GetCitasActivasEntreAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Cita>
                {
                    CrearCita(EstadoCita.Confirmada, new DateTime(2026, 10, 6, 11, 0, 0), new DateTime(2026, 10, 6, 12, 0, 0))
                });

            // Reserva suave vigente dentro de la semana
            _mockReservasSuaves.Setup(r => r.ObtenerVigentes())
                .Returns(new List<ReservaSuaveDto>
                {
                    new(CitaFechas.AUtc(new DateTime(2026, 10, 6, 15, 0, 0)),
                        CitaFechas.AUtc(new DateTime(2026, 10, 6, 15, 30, 0)))
                });

            // Captura del rango UTC que el handler pide al repositorio
            var iniciosConsultados = new List<DateTimeOffset>();
            var finesConsultados = new List<DateTimeOffset>();
            _mockAgendaRepository
                .Setup(r => r.GetBloqueosClinicaEntreAsync(Capture.In(iniciosConsultados), Capture.In(finesConsultados)))
                .ReturnsAsync(new List<Bloqueo>
                {
                    new()
                    {
                        Motivo = "Capacitación del personal",
                        FechaInicio = CitaFechas.AUtc(new DateTime(2026, 10, 7, 9, 0, 0)),
                        FechaFin = CitaFechas.AUtc(new DateTime(2026, 10, 7, 13, 0, 0)),
                        PersonalId = null
                    }
                });

            // Act
            var agenda = await CrearHandler().Handle(new GetAgendaSemanaQuery
            {
                SemanaInicioLocal = SemanaInicio,
                SemanaFinLocal = SemanaFin
            }, CancellationToken.None);

            // Assert: una ocupacion de cada estado, sin canceladas ni fuera de rango
            agenda.Ocupaciones.Count(o => o.Estado == nameof(EstadoSlot.Ocupado)).ShouldBe(1);
            agenda.Ocupaciones.Count(o => o.Estado == nameof(EstadoSlot.Reservando)).ShouldBe(1);
            agenda.Ocupaciones.Count(o => o.Estado == nameof(EstadoSlot.Bloqueado)).ShouldBe(1);

            // El bloqueo viene con el rango del bloqueo sembrado (miercoles 9-13)
            var bloqueo = agenda.Ocupaciones.Single(o => o.Estado == nameof(EstadoSlot.Bloqueado));
            bloqueo.FechaInicio.ShouldBe(CitaFechas.AUtc(new DateTime(2026, 10, 7, 9, 0, 0)));
            bloqueo.FechaFin.ShouldBe(CitaFechas.AUtc(new DateTime(2026, 10, 7, 13, 0, 0)));

            // La consulta de bloqueos se hace con la semana convertida a UTC
            iniciosConsultados.Single().ShouldBe(CitaFechas.AUtc(SemanaInicio));
            finesConsultados.Single().ShouldBe(CitaFechas.AUtc(SemanaFin));
        }
    }
}
