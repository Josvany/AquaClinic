using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    /// <summary>
    /// Refresh tokens emitidos por la API. Se guardan hasheados para que la sesion
    /// sobreviva a reinicios del proceso de la API.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>Registra un token recien emitido (guarda su hash).</summary>
        Task AddAsync(RefreshToken refreshToken);

        /// <summary>
        /// Busca un token por su hash. Null si no existe (o ya fue rotado: la
        /// rotacion es de un solo uso y borra la fila).
        /// </summary>
        Task<RefreshToken?> GetPorHashAsync(string tokenHash);

        /// <summary>
        /// Rota el token: lo borra y devuelve el UsuarioId. Null si el token no
        /// existe o ya vencio. El servicio serializa las llamadas (SemaphoreSlim)
        /// para que un token no pueda canjearse dos veces.
        /// </summary>
        Task<string?> RotarAsync(string tokenHash, DateTime ahoraUtc);

        /// <summary>Revoca (borra) todos los tokens del usuario: logout.</summary>
        Task RevocarTodosAsync(string usuarioId);

        /// <summary>Borra los tokens ya vencidos. Purga de mantenimiento.</summary>
        Task<int> PurgarVencidosAsync(DateTime ahoraUtc);
    }
}