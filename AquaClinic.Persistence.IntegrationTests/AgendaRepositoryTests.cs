using AquaClinic.Domain.Entities;
using AquaClinic.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;

namespace AquaClinic.Persistence.IntegrationTests
{
    /// <summary>
    /// Verifica la consulta de bloqueos de clinica sobre la base de datos:
    /// solo devuelve filas con PersonalId = null (los bloqueos de una persona no
    /// cierran la clinica) y el solape es estricto (los rangos que solo se tocan
    /// en el borde no cuentan como solapados).
    /// </summary>
    public class AgendaRepositoryTests
    {
        private static readonly TimeSpan OffsetPdt = TimeSpan.FromHours(-7);

        private readonly AquaClinicDbContext _dbContext;
        private readonly AgendaRepository _repository;

        public AgendaRepositoryTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AquaClinicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var loggedInUserServiceMock = new Mock<Application.Contracts.ILoggedInUserService>();
            loggedInUserServiceMock.Setup(m => m.UserId).Returns("usuario-de-pruebas");

            _dbContext = new AquaClinicDbContext(dbContextOptions, loggedInUserServiceMock.Object);
            _repository = new AgendaRepository(_dbContext);
        }

        private async Task SembrarBloqueosAsync()
        {
            // Escenario espejo del seed de produccion: un bloqueo de toda la
            // clinica y uno de una esteticista en el mismo rango del dia.
            _dbContext.Bloqueos.AddRange(
                new Bloqueo
                {
                    Motivo = "Capacitación del equipo",
                    FechaInicio = new DateTimeOffset(2026, 10, 5, 9, 0, 0, OffsetPdt),
                    FechaFin = new DateTimeOffset(2026, 10, 5, 17, 0, 0, OffsetPdt),
                    PersonalId = null
                },
                new Bloqueo
                {
                    Motivo = "Vacaciones de la esteticista",
                    FechaInicio = new DateTimeOffset(2026, 10, 5, 10, 0, 0, OffsetPdt),
                    FechaFin = new DateTimeOffset(2026, 10, 5, 12, 0, 0, OffsetPdt),
                    PersonalId = Guid.NewGuid()
                });
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetBloqueosClinicaEntreAsync_SoloDevuelveBloqueosDeClinicaQueSolapan()
        {
            await SembrarBloqueosAsync();

            // 10 a 11 se solapa con ambos bloqueos, pero solo el de clinica cuenta
            var resultado = await _repository.GetBloqueosClinicaEntreAsync(
                new DateTimeOffset(2026, 10, 5, 10, 0, 0, OffsetPdt),
                new DateTimeOffset(2026, 10, 5, 11, 0, 0, OffsetPdt));

            resultado.ShouldHaveSingleItem();
            resultado.Single().Motivo.ShouldBe("Capacitación del equipo");
            resultado.Single().PersonalId.ShouldBeNull();
        }

        [Fact]
        public async Task GetBloqueosClinicaEntreAsync_LimitesAdyacentesNoCuentanComoSolape()
        {
            await SembrarBloqueosAsync();

            // Termina justo cuando inicia el bloqueo (9:00)
            var antes = await _repository.GetBloqueosClinicaEntreAsync(
                new DateTimeOffset(2026, 10, 5, 8, 0, 0, OffsetPdt),
                new DateTimeOffset(2026, 10, 5, 9, 0, 0, OffsetPdt));
            antes.ShouldBeEmpty();

            // Empieza justo cuando termina el bloqueo (17:00)
            var despues = await _repository.GetBloqueosClinicaEntreAsync(
                new DateTimeOffset(2026, 10, 5, 17, 0, 0, OffsetPdt),
                new DateTimeOffset(2026, 10, 5, 18, 0, 0, OffsetPdt));
            despues.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetBloqueosClinicaEntreAsync_RangosContenidosYQueContienenCuentanComoSolape()
        {
            await SembrarBloqueosAsync();

            // La cita queda dentro del bloqueo
            var contenido = await _repository.GetBloqueosClinicaEntreAsync(
                new DateTimeOffset(2026, 10, 5, 12, 0, 0, OffsetPdt),
                new DateTimeOffset(2026, 10, 5, 13, 0, 0, OffsetPdt));
            contenido.ShouldHaveSingleItem();

            // La cita cubre el bloqueo completo
            var queContiene = await _repository.GetBloqueosClinicaEntreAsync(
                new DateTimeOffset(2026, 10, 5, 8, 0, 0, OffsetPdt),
                new DateTimeOffset(2026, 10, 5, 18, 0, 0, OffsetPdt));
            queContiene.ShouldHaveSingleItem();
        }

        [Fact]
        public async Task GetBloqueosClinicaEntreAsync_BloqueoEnOtraFecha_NoSeDevuelve()
        {
            await SembrarBloqueosAsync();

            var otraSemana = await _repository.GetBloqueosClinicaEntreAsync(
                new DateTimeOffset(2026, 10, 12, 9, 0, 0, OffsetPdt),
                new DateTimeOffset(2026, 10, 12, 17, 0, 0, OffsetPdt));

            otraSemana.ShouldBeEmpty();
        }
    }
}
