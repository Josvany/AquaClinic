using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Persistence.Repositories
{
    /// <summary>
    /// Refresh tokens sobre la BD. Se usan borrados rastreados (no ExecuteDelete)
    /// para que el repositorio funcione con cualquier provider, incluido el
    /// InMemory de los tests.
    ///
    /// La garantia de "un solo uso" NO viene de aqui: la aporta el
    /// SemaphoreSlim de TokenService, que serializa las rotaciones dentro del
    /// proceso. Con varias instancias de la API concurrentes habria que reforzarlo
    /// a nivel de BD (token de concurrencia o ExecuteDelete condicional).
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AquaClinicDbContext _dbContext;

        public RefreshTokenRepository(AquaClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetPorHashAsync(string tokenHash)
        {
            return await _dbContext.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task<string?> RotarAsync(string tokenHash, DateTime ahoraUtc)
        {
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (token is null || token.ExpiraUtc < ahoraUtc)
            {
                return null;
            }

            _dbContext.RefreshTokens.Remove(token);
            await _dbContext.SaveChangesAsync();

            return token.UsuarioId;
        }

        public async Task RevocarTodosAsync(string usuarioId)
        {
            var tokens = await _dbContext.RefreshTokens
                .Where(t => t.UsuarioId == usuarioId)
                .ToListAsync();

            _dbContext.RefreshTokens.RemoveRange(tokens);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> PurgarVencidosAsync(DateTime ahoraUtc)
        {
            var vencidos = await _dbContext.RefreshTokens
                .Where(t => t.ExpiraUtc < ahoraUtc)
                .ToListAsync();

            _dbContext.RefreshTokens.RemoveRange(vencidos);
            await _dbContext.SaveChangesAsync();

            return vencidos.Count;
        }
    }
}