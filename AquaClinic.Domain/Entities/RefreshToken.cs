namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Refresh token emitido por la API, persistido para que la sesion sobreviva a
    /// reinicios de la API. Antes vivian en un diccionario estatico en memoria, y
    /// cualquier reinicio del proceso los perdia.
    ///
    /// IMPORTANTE: aqui NUNCA se guarda el token en claro, solo su SHA-256. El
    /// token en claro es una credencial de tipo bearer: si se filtrara la base de
    /// datos, con el hash no se podria suplantar a nadie. El indice unico sobre
    /// TokenHash permite validar por busqueda directa.
    /// </summary>
    public class RefreshToken
    {
        public RefreshToken()
        {
            RefreshTokenId = Guid.CreateVersion7();
            CreadoUtc = DateTime.UtcNow;
        }

        public Guid RefreshTokenId { get; set; }

        /// <summary>SHA-256 en base64 del token entregado al cliente.</summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>Usuario al que pertenece el token.</summary>
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>Caduca a los 7 dias de emitirse (Jwt:RefreshTokenDias).</summary>
        public DateTime ExpiraUtc { get; set; }

        public DateTime CreadoUtc { get; set; }

        /// <summary>
        /// Ultimo uso del token, para purgar los que ya no sirven sin borrar sus
        /// filas de golpe (un DELETE masivo cada login seria una pena).
        /// </summary>
        public DateTime? UltimoUsoUtc { get; set; }
    }
}