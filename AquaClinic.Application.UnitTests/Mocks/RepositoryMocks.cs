using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Moq;

namespace AquaClinic.Application.UnitTests.Mocks
{
    public class RepositoryMocks
    {
        public static Mock<IAsyncRepository<Especialidad>> GetEspecialidadRepository()
        {
            var cardiologiaGuid = Guid.Parse("{B0788D2F-8003-43C1-92A4-EDC76A7C5DDE}");
            var pediatriaGuid = Guid.Parse("{6313179F-7837-473A-A4D5-A5571B43E6A6}");
            var dermatologiaGuid = Guid.Parse("{BF3F3002-7E53-441E-8B76-F6280BE284AA}");
            var traumatologiaGuid = Guid.Parse("{FE98F549-E790-4E9F-AA16-18C2292A2EE9}");

            var categories = new List<Especialidad>
            {
                new Especialidad
                {
                    EspecialidadId = cardiologiaGuid,
                    Nombre = "Cardiología"
                },
                new Especialidad
                {
                    EspecialidadId = pediatriaGuid,
                    Nombre = "Pediatría"
                },
                new Especialidad
                {
                    EspecialidadId = traumatologiaGuid,
                    Nombre = "Traumatología"
                },
                 new Especialidad
                {
                    EspecialidadId = dermatologiaGuid,
                    Nombre = "Dermatología"
                }
            };

            var mockEspecialidadRepository = new Mock<IAsyncRepository<Especialidad>>();
            mockEspecialidadRepository.Setup(repo => repo.ListAllAsync()).ReturnsAsync(categories);

            mockEspecialidadRepository.Setup(repo => repo.AddAsync(It.IsAny<Especialidad>())).ReturnsAsync(
                (Especialidad category) =>
                {
                    categories.Add(category);
                    return category;
                });

            return mockEspecialidadRepository;
        }
    }
}
