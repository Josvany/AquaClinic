using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AquaClinic.Identity
{
    /// <summary>
    /// Emite access tokens JWT (con los roles como claims) y refresh tokens
    /// opacos almacenados en memoria compartida (singleton: los tokens deben
    /// sobrevivir entre requests, que son scoped). El JWT es stateless.
    /// </summary>
    public class TokenService(
        IConfiguration configuration)
    {
        private static readonly Dictionary<string, (string UsuarioId, DateTime Expira)> _refreshTokens = new();
        private static readonly object _lock = new();

        public double MinutosVidaAccessToken =>
            configuration.GetValue("Jwt:AccessTokenMinutos", 60);

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

        /// <summary>Emite y almacena un refresh token opaco (validez 7 dias).</summary>
        public string EmitirRefreshToken(string usuarioId)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            lock (_lock)
            {
                _refreshTokens[token] = (usuarioId, DateTime.UtcNow.AddDays(7));

                // Limpieza de tokens vencidos
                foreach (var vencido in _refreshTokens
                    .Where(kvp => kvp.Value.Expira < DateTime.UtcNow)
                    .Select(kvp => kvp.Key))
                {
                    _refreshTokens.Remove(vencido);
                }
            }

            return token;
        }

        /// <summary>
        /// Valida un refresh token y lo rota (un solo uso). Devuelve el UsuarioId
        /// o null si el token no existe, vencio o ya fue consumido.
        /// </summary>
        public string? ValidarYRotarRefreshToken(string refreshToken)
        {
            lock (_lock)
            {
                if (!_refreshTokens.TryGetValue(refreshToken, out var dato))
                {
                    return null;
                }

                _refreshTokens.Remove(refreshToken);

                return dato.Expira >= DateTime.UtcNow ? dato.UsuarioId : null;
            }
        }

        /// <summary>Revoca todos los refresh tokens del usuario (logout).</summary>
        public void RevocarRefreshTokens(string usuarioId)
        {
            lock (_lock)
            {
                foreach (var token in _refreshTokens
                    .Where(kvp => kvp.Value.UsuarioId == usuarioId)
                    .Select(kvp => kvp.Key))
                {
                    _refreshTokens.Remove(token);
                }
            }
        }

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
