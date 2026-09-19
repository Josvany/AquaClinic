using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList;
using AquaClinic.Application.Profiles;
using AquaClinic.Application.UnitTests.Mocks;
using AquaClinic.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace AquaClinic.Application.UnitTests.Especialidades.Queries
{
    public class GetEspecialidadesListQueryHandlerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IAsyncRepository<Especialidad>> _mockEspecialidadRepository;

        public GetEspecialidadesListQueryHandlerTests()
        {
            _mockEspecialidadRepository = RepositoryMocks.GetEspecialidadRepository();
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, NullLoggerFactory.Instance);

            _mapper = configurationProvider.CreateMapper();
        }

        [Fact]
        public async Task GetEspecialidadesListTest()
        {
            var handler = new GetEspecialidadesListQueryHandler(_mapper, _mockEspecialidadRepository.Object);

            var result = await handler.Handle(new GetEspecialidadesListQuery(), CancellationToken.None);

            result.ShouldBeOfType<List<EspecialidadListVm>>();

            result.Count.ShouldBe(4);
        }
    }
}
