using AquaClinic.Domain.Entities;
using AquaClinic.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;

namespace AquaClinic.Persistence.IntegrationTests
{
    /// <summary>
    /// Refresh tokens persistidos. Fija las tres propiedades de seguridad:
    /// se guarda el HASH (nunca el token en claro), la rotacion es de UN SOLO
    /// uso y el logout revoca todos los tokens del usuario.
    /// </summary>
    public class RefreshTokenRepositoryTests
    {
        private static readonly DateTime Ahora = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

        private readonly AquaClinicDbContext _dbContext;
        private readonly RefreshTokenRepository _repository;

        public RefreshTokenRepositoryTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AquaClinicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var loggedInUserServiceMock = new Mock<Application.Contracts.ILoggedInUserService>();
            loggedInUserServiceMock.Setup(m => m.UserId).Returns("usuario-de-pruebas");

            _dbContext = new AquaClinicDbContext(dbContextOptions, loggedInUserServiceMock.Object);
            _repository = new RefreshTokenRepository(_dbContext);
        }

        [Fact]
        public async Task Guardar_No_Deja_El_Token_En_CClaro()
        {
            const string tokenEnClaro = "token-secreto-del-usuario";

            await _repository.AddAsync(new RefreshToken
            {
                TokenHash = Hashear(tokenEnClaro),
                UsuarioId = "u1",
                ExpiraUtc = Ahora.AddDays(7)
            });

            // Nadie debe poder recuperar el token original desde la BD
            var filas = _dbContext.RefreshTokens.AsNoTracking().ToList();
            filas.ShouldHaveSingleItem().TokenHash.ShouldNotBe(tokenEnClaro);
        }

        [Fact]
        public async Task Rotar_Devuelve_El_Usuario_Y_Consume_El_Token()
        {
            var hash = Hashear("r1");
            await _repository.AddAsync(new RefreshToken { TokenHash = hash, UsuarioId = "u1", ExpiraUtc = Ahora.AddDays(7) });

            (await _repository.RotarAsync(hash, Ahora)).ShouldBe("u1");

            // Un solo uso: el mismo token ya no vale
            (await _repository.RotarAsync(hash, Ahora)).ShouldBeNull();
            (await _repository.GetPorHashAsync(hash)).ShouldBeNull();
        }

        [Fact]
        public async Task Rotar_Un_Token_Vencido_No_Devuelve_Usuario()
        {
            var hash = Hashear("vencido");
            await _repository.AddAsync(new RefreshToken { TokenHash = hash, UsuarioId = "u1", ExpiraUtc = Ahora.AddHours(-1) });

            (await _repository.RotarAsync(hash, Ahora)).ShouldBeNull();
        }

        [Fact]
        public async Task Rotar_Un_Token_Inexistente_No_Devuelve_Usuario()
        {
            (await _repository.RotarAsync(Hashear("nunca-existio"), Ahora)).ShouldBeNull();
        }

        [Fact]
        public async Task RevocarTodos_Borra_Solo_Los_Del_Usuario()
        {
            await _repository.AddAsync(new RefreshToken { TokenHash = Hashear("a"), UsuarioId = "u1", ExpiraUtc = Ahora.AddDays(7) });
            await _repository.AddAsync(new RefreshToken { TokenHash = Hashear("b"), UsuarioId = "u1", ExpiraUtc = Ahora.AddDays(7) });
            await _repository.AddAsync(new RefreshToken { TokenHash = Hashear("c"), UsuarioId = "u2", ExpiraUtc = Ahora.AddDays(7) });

            await _repository.RevocarTodosAsync("u1");

            (await _repository.GetPorHashAsync(Hashear("a"))).ShouldBeNull();
            (await _repository.GetPorHashAsync(Hashear("b"))).ShouldBeNull();
            // La sesion del otro usuario no se toca
            (await _repository.GetPorHashAsync(Hashear("c"))).ShouldNotBeNull();
        }

        [Fact]
        public async Task PurgarVencidos_Borra_Solo_Los_Caducados()
        {
            await _repository.AddAsync(new RefreshToken { TokenHash = Hashear("viejo"), UsuarioId = "u1", ExpiraUtc = Ahora.AddHours(-1) });
            await _repository.AddAsync(new RefreshToken { TokenHash = Hashear("nuevo"), UsuarioId = "u1", ExpiraUtc = Ahora.AddDays(7) });

            (await _repository.PurgarVencidosAsync(Ahora)).ShouldBe(1);

            (await _repository.GetPorHashAsync(Hashear("viejo"))).ShouldBeNull();
            (await _repository.GetPorHashAsync(Hashear("nuevo"))).ShouldNotBeNull();
        }

        /// <summary>Mismo hash que calcula TokenService al emitir.</summary>
        private static string Hashear(string token) =>
            Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token)));
    }
}