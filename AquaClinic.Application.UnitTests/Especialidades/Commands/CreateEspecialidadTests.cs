using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using AquaClinic.Application.Profiles;
using AquaClinic.Application.UnitTests.Mocks;
using AquaClinic.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace AquaClinic.Application.UnitTests.Especialidades.Commands
{
    public class CreateEspecialidadTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IAsyncRepository<Especialidad>> _mockEspecialidadRepository;

        public CreateEspecialidadTests()
        {
            _mockEspecialidadRepository = RepositoryMocks.GetEspecialidadRepository();
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, NullLoggerFactory.Instance);

            _mapper = configurationProvider.CreateMapper();
        }

        [Fact]
        public async Task Handle_ValidEspecialidad_AddedToEspecialidadesRepo()
        {
            var handler = new CreateEspecialidadCommandHandler(_mapper, _mockEspecialidadRepository.Object);

            await handler.Handle(new CreateEspecialidadCommand() { Nombre = "Prueba" }, CancellationToken.None);

            var allEspecialidades = await _mockEspecialidadRepository.Object.ListAllAsync();
            allEspecialidades.Count.ShouldBe(5);
        }
    }
}
