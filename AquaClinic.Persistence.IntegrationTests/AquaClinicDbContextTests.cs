using AquaClinic.Application.Contracts;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;

namespace AquaClinic.Persistence.IntegrationTests
{
    public class AquaClinicDbContextTests
    {
        private readonly AquaClinicDbContext _aquaClinicDbContext;
        private readonly Mock<ILoggedInUserService> _loggedInUserServiceMock;
        private readonly string _loggedInUserId;

        public AquaClinicDbContextTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AquaClinicDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            _loggedInUserId = "00000000-0000-0000-0000-000000000000";
            _loggedInUserServiceMock = new Mock<ILoggedInUserService>();
            _loggedInUserServiceMock.Setup(m => m.UserId).Returns(_loggedInUserId);

            _aquaClinicDbContext = new AquaClinicDbContext(dbContextOptions, _loggedInUserServiceMock.Object);
        }

        [Fact]
        public async Task Save_SetCreatedByProperty()
        {
            var servicio = new Servicio() { ServicioId = Guid.NewGuid(), Nombre = "Servicio de prueba" };

            _aquaClinicDbContext.Servicios.Add(servicio);
            await _aquaClinicDbContext.SaveChangesAsync();

            servicio.CreatedBy.ShouldBe(_loggedInUserId);
        }
    }
}