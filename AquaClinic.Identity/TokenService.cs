using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AquaClinic.Identity
{
    /// <summary>
    /// Emite access tokens JWT (con los roles como claims) y refresh tokens
    /// opacos persistidos en la BD.
    ///
    /// OJO: los refresh tokens vivian antes en un diccionario estatico EN MEMORIA,
    /// lo que hacia que reiniciar la API cerrara todas las sesiones aunque el
    /// access token siguiera vigente. Ahora se guardan hasheados en la tabla
    /// RefreshTokens (nunca en claro), asi que sobreviven a reinicios.
    ///
    /// El access token JWT sigue siendo stateless.
    /// </summary>
    public class TokenService(
        IConfiguration configuration,
        IRefreshTokenRepository refreshTokens)
    {
        /// <summary>
        /// Serializa emitir / rotar / revocar refresh tokens. La rotacion es de UN
        /// SOLO USO: sin esto, dos peticiones simultaneas con el mismo token (la
        /// ruta SSR y el circuito Blazor pueden renovar a la vez) podrian canjearlo
        /// las dos y dejar una sesion huerfana con un token ya consumido.
        ///
        /// SemaphoreSlim y no lock porque las operaciones son async: un lock
        /// sostenido sobre un await puede deadlear.
        /// </summary>
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        public double MinutosVidaAccessToken =>
            configuration.GetValue("Jwt:AccessTokenMinutos", 60);

        public double DiasVidaRefreshToken =>
            configuration.GetValue("Jwt:RefreshTokenDias", 7);

        /// <summary>
        /// Genera el JWT del usuario con claims: sub, email, nombre y un claim de
        /// rol por cada rol asignado (mismo ClaimTypes.Role que usaba la cookie,
        /// para que PermissionProvider siga funcionando sin cambios).
        /// </summary>
        public async Task<(string Token, DateTime Expira)> EmitirAccessTokenAsync(
            string usuarioId, string email, IEnumerable<string> roles, string? nombre)
        {
            var clave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(ObtenerClave()));

            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddMinutes(MinutosVidaAccessToken);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuarioId),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new(ClaimTypes.NameIdentifier, usuarioId),
                new(ClaimTypes.Name, email)
            };

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                claims.Add(new Claim("nombre", nombre));
            }

            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiracion,
                signingCredentials: credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
        }

        /// <summary>
        /// Emite un refresh token opaco y lo registra hasheado en la BD.
        /// Purgar de vez en cuando los vencidos para que la tabla no crezca sin
        /// control (con un DELETE, no en el camino caliente del login).
        /// </summary>
        public async Task<string> EmitirRefreshTokenAsync(string usuarioId)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            await _refreshLock.WaitAsync();
            try
            {
                await refreshTokens.AddAsync(new RefreshToken
                {
                    TokenHash = Hashear(token),
                    UsuarioId = usuarioId,
                    ExpiraUtc = DateTime.UtcNow.AddDays(DiasVidaRefreshToken)
                });
            }
            finally
            {
                _refreshLock.Release();
            }

            return token;
        }

        /// <summary>
        /// Valida un refresh token y lo rota (un solo uso). Devuelve el UsuarioId
        /// o null si el token no existe, vencio o ya fue consumido.
        /// </summary>
        public async Task<string?> ValidarYRotarRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            await _refreshLock.WaitAsync();
            try
            {
                return await refreshTokens.RotarAsync(Hashear(refreshToken), DateTime.UtcNow);
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        /// <summary>Revoca todos los refresh tokens del usuario (logout).</summary>
        public async Task RevocarRefreshTokensAsync(string usuarioId)
        {
            await _refreshLock.WaitAsync();
            try
            {
                await refreshTokens.RevocarTodosAsync(usuarioId);
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        /// <summary>Borra los refresh tokens vencidos (mantenimiento).</summary>
        public Task<int> PurgarRefreshTokensVencidosAsync() =>
            refreshTokens.PurgarVencidosAsync(DateTime.UtcNow);

        /// <summary>
        /// SHA-256 en base64 del token. Se guarda el hash porque el token en claro
        /// es una credencial bearer: si se filtra la BD no debe servir para
        /// suplantar a nadie. Busca rapido por indice unico.
        /// </summary>
        private static string Hashear(string token) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private string ObtenerClave()
        {
            var clave = configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 32)
            {
                throw new InvalidOperationException(
                    "La clave JWT no esta configurada (Jwt:Key). Definela con al menos 32 caracteres " +
                    "en appsettings (Development) o por variable de entorno Jwt__Key (produccion).");
            }

            return clave;
        }
    }
}
