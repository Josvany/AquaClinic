using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Application.Features.Bloqueos.Commands.CreateBloqueo;
using AquaClinic.Application.Features.Bloqueos.Commands.DeleteBloqueo;
using AquaClinic.Application.Features.Bloqueos.Commands.UpdateBloqueo;
using AquaClinic.Application.Features.Bloqueos.Queries.GetBloqueosList;
using AquaClinic.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace AquaClinic.Application.UnitTests.Bloqueos.Commands
{
    /// <summary>
    /// Verifica el CRUD de bloqueos de la clinica: crea con PersonalId = null,
    /// actualiza solo motivo y rango, elimina fisicamente (sin historial) y
    /// responde NotFound / validacion cuando corresponde.
    /// </summary>
    public class BloqueoCrudTests
    {
        private readonly Mock<IBloqueoRepository> _mockBloqueoRepository;

        /// <summary>Bloqueo de ejemplo 9-17 de hora local de la clinica (PDT -07:00), en UTC.</summary>
        private static readonly DateTimeOffset InicioUtc = new(2026, 12, 24, 16, 0, 0, TimeSpan.FromHours(-7));
        private static readonly DateTimeOffset FinUtc = new(2026, 12, 24, 17, 0, 0, TimeSpan.FromHours(-7));

        public BloqueoCrudTests()
        {
            _mockBloqueoRepository = new Mock<IBloqueoRepository>();
        }

        [Fact]
        public async Task Handle_CreaBloqueoDeClinicaSinPersonaYDevuelveSuId()
        {
            // Arrange
            _mockBloqueoRepository.Setup(r => r.AddAsync(It.IsAny<Bloqueo>()))
                .ReturnsAsync((Bloqueo b) => b);

            // Act
            var id = await new CreateBloqueoCommandHandler(ObtenerMapper(), _mockBloqueoRepository.Object)
                .Handle(new CreateBloqueoCommand
                {
                    Motivo = "Navidad",
                    FechaInicio = InicioUtc,
                    FechaFin = FinUtc
                }, CancellationToken.None);

            // Assert: el id devuelto es el v7 generado por la entidad y se persiste
            // un bloqueo de toda la clinica con el rango pedido
            id.ShouldNotBe(Guid.Empty);
            _mockBloqueoRepository.Verify(r => r.AddAsync(It.Is<Bloqueo>(b =>
                b.PersonalId == null &&
                b.Motivo == "Navidad" &&
                b.FechaInicio == InicioUtc &&
                b.FechaFin == FinUtc)), Times.Once);
        }

        [Fact]
        public async Task Handle_CreaRechazaRangoInvalidoSinConsultarRepositorio()
        {
            // Arrange: fin <= inicio
            var comando = new CreateBloqueoCommand
            {
                Motivo = "Cierre invertido",
                FechaInicio = FinUtc,
                FechaFin = InicioUtc
            };

            // Act
            var excepcion = await Should.ThrowAsync<ValidationException>(() =>
                new CreateBloqueoCommandHandler(ObtenerMapper(), _mockBloqueoRepository.Object)
                    .Handle(comando, CancellationToken.None));

            // Assert
            excepcion.ValdationErrors.ShouldContain(e => e.Contains("posterior a la fecha de inicio"));
            _mockBloqueoRepository.Verify(r => r.AddAsync(It.IsAny<Bloqueo>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CreaRechazaMotivoVacioSinConsultarRepositorio()
        {
            // Arrange
            var comando = new CreateBloqueoCommand
            {
                Motivo = "   ",
                FechaInicio = InicioUtc,
                FechaFin = FinUtc
            };

            // Act
            var excepcion = await Should.ThrowAsync<ValidationException>(() =>
                new CreateBloqueoCommandHandler(ObtenerMapper(), _mockBloqueoRepository.Object)
                    .Handle(comando, CancellationToken.None));

            // Assert
            excepcion.ValdationErrors.ShouldContain(e => e.Contains("Motivo"));
            _mockBloqueoRepository.Verify(r => r.AddAsync(It.IsAny<Bloqueo>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ActualizaMotivoYRangoDeUnBloqueoExistente()
        {
            // Arrange
            var bloqueo = new Bloqueo
            {
                Motivo = "Original",
                FechaInicio = InicioUtc,
                FechaFin = FinUtc
            };
            _mockBloqueoRepository.Setup(r => r.GetByIdAsync(bloqueo.BloqueoId))
                .ReturnsAsync(bloqueo);

            var comando = new UpdateBloqueoCommand
            {
                BloqueoId = bloqueo.BloqueoId,
                Motivo = "Feriado Navidad",
                FechaInicio = InicioUtc.AddDays(1),
                FechaFin = FinUtc.AddDays(1)
            };

            // Act
            await new UpdateBloqueoCommandHandler(ObtenerMapper(), _mockBloqueoRepository.Object)
                .Handle(comando, CancellationToken.None);

            // Assert: la entidad mapeada se persiste con los nuevos valores
            _mockBloqueoRepository.Verify(r => r.UpdateAsync(It.Is<Bloqueo>(b =>
                b.BloqueoId == bloqueo.BloqueoId &&
                b.Motivo == "Feriado Navidad" &&
                b.FechaInicio == InicioUtc.AddDays(1) &&
                b.FechaFin == FinUtc.AddDays(1) &&
                b.PersonalId == null)), Times.Once);
        }

        [Fact]
        public async Task Handle_ActualizaLanzaNotFoundSiElBloqueoNoExiste()
        {
            // Arrange
            _mockBloqueoRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Bloqueo?)null);

            var comando = new UpdateBloqueoCommand
            {
                BloqueoId = Guid.NewGuid(),
                Motivo = "Cualquier motivo",
                FechaInicio = InicioUtc,
                FechaFin = FinUtc
            };

            // Act / Assert
            await Should.ThrowAsync<NotFoundException>(() =>
                new UpdateBloqueoCommandHandler(ObtenerMapper(), _mockBloqueoRepository.Object)
                    .Handle(comando, CancellationToken.None));

            _mockBloqueoRepository.Verify(r => r.UpdateAsync(It.IsAny<Bloqueo>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EliminaFisicamenteUnBloqueoExistente()
        {
            // Arrange: los bloqueos no tienen historial asociado (las citas no los referencian)
            var bloqueo = new Bloqueo
            {
                Motivo = "Capacitación",
                FechaInicio = InicioUtc,
                FechaFin = FinUtc
            };
            _mockBloqueoRepository.Setup(r => r.GetByIdAsync(bloqueo.BloqueoId))
                .ReturnsAsync(bloqueo);

            // Act
            await new DeleteBloqueoCommandHandler(_mockBloqueoRepository.Object)
                .Handle(new DeleteBloqueoCommand { BloqueoId = bloqueo.BloqueoId }, CancellationToken.None);

            // Assert: el handler no implementa borrado logico: la fila se borra
            _mockBloqueoRepository.Verify(r => r.DeleteAsync(bloqueo), Times.Once);
            _mockBloqueoRepository.Verify(r => r.UpdateAsync(It.IsAny<Bloqueo>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EliminaLanzaNotFoundSiElBloqueoNoExiste()
        {
            // Arrange
            _mockBloqueoRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Bloqueo?)null);

            // Act / Assert
            await Should.ThrowAsync<NotFoundException>(() =>
                new DeleteBloqueoCommandHandler(_mockBloqueoRepository.Object)
                    .Handle(new DeleteBloqueoCommand { BloqueoId = Guid.NewGuid() }, CancellationToken.None));

            _mockBloqueoRepository.Verify(r => r.DeleteAsync(It.IsAny<Bloqueo>()), Times.Never);
        }

        // ---------- Helpers ----------

        private static IMapper ObtenerMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Profiles.MappingProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }
    }
}
