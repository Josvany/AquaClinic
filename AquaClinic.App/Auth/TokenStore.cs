namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Almacen de la sesion JWT en el servidor Blazor, por circuito interactivo
    /// (scoped). Reemplaza al almacen de cookies: guarda el access token y el
    /// refresh token que la API emitio al hacer login, para inyectarlos como
    /// Authorization: Bearer en cada llamada REST (ver AuthHeaderHandler).
    /// Se pierde al reiniciar el servidor: el usuario vuelve a iniciar sesion.
    /// </summary>
    public class TokenStore
    {
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        public DateTime ExpiraUtc { get; private set; }
        public string? Email { get; private set; }

        public bool TieneToken => !string.IsNullOrEmpty(AccessToken);

        public void Guardar(string accessToken, string refreshToken, DateTime expiraUtc, string email)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiraUtc = expiraUtc;
            Email = email;
        }

        /// <summary>Reemplaza solo el access token tras un refresh exitoso.</summary>
        public void ActualizarAccessToken(string accessToken, DateTime expiraUtc)
        {
            AccessToken = accessToken;
            ExpiraUtc = expiraUtc;
        }

        /// <summary>Reemplaza solo el refresh token (rotacion en refresh).</summary>
        public void GuardarRefresh(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public void Limpiar()
        {
            AccessToken = null;
            RefreshToken = null;
            ExpiraUtc = default;
            Email = null;
        }
    }
}
