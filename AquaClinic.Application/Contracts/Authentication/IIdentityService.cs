using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Authentication
{
    /// <summary>
    /// Resultado de una operacion de identity: &#233;xito o lista de errores legibles.
    /// </summary>
    public sealed record ResultadoIdentity(bool Exito, IReadOnlyList<string> Errores)
    {
        public static ResultadoIdentity Ok() => new(true, []);
        public static ResultadoIdentity Fallo(params string[] errores) => new(false, errores);
    }

    /// <summary>
    /// Resultado del registro: incluye el Id del usuario creado.
    /// </summary>
    public sealed record ResultadoRegistro(bool Exito, IReadOnlyList<string> Errores, string? UsuarioId = null)
    {
        public static ResultadoRegistro Ok(string usuarioId) => new(true, [], usuarioId);
        public static ResultadoRegistro Fallo(params string[] errores) => new(false, errores);
    }

    /// <summary>
    /// Abstraccion de las operaciones de cuenta de usuario. La implementa la capa
    /// de Identity con UserManager<ApplicationUser>; los handlers de Application
    /// dependen solo de esta interfaz (no de ASP.NET Identity).
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>Registra un cliente nuevo (rol Cliente). Devuelve el UsuarioId.</summary>
        Task<ResultadoRegistro> RegistrarAsync(string email, string password, string? nombre, string? apellidos, string? telefono, bool aceptaComunicaciones = false);

        /// <summary>Valida credenciales. Devuelve el usuario con roles, o null si son inv&#225;lidas.</summary>
        Task<Usuario?> ValidarCredencialesAsync(string email, string password);

        /// <summary>Obtiene el usuario con roles por Id.</summary>
        Task<Usuario?> ObtenerPorIdAsync(string usuarioId);

        /// <summary>
        /// Busca clientes registrados (rol Cliente) por nombre, apellidos o correo.
        /// Sin texto devuelve los primeros ordenados por nombre.
        /// </summary>
        Task<IReadOnlyList<Usuario>> BuscarClientesAsync(string? texto, int limite);

        /// <summary>Cambia la contrase&#241;a verificando la actual.</summary>
        Task<ResultadoIdentity> CambiarPasswordAsync(string usuarioId, string passwordActual, string passwordNueva);

        /// <summary>Actualiza el perfil (nombre, apellidos, tel&#233;fono).</summary>
        Task<ResultadoIdentity> ActualizarPerfilAsync(string usuarioId, string? nombre, string? apellidos, string? telefono);

        /// <summary>Genera el token de restablecimiento. Null si el correo no existe.</summary>
        Task<string?> GenerarTokenRestablecimientoAsync(string email);

        /// <summary>Restablece la contrase&#241;a con el token de un solo uso.</summary>
        Task<ResultadoIdentity> RestablecerPasswordAsync(string email, string token, string passwordNueva);

        /// <summary>Elimina la cuenta del usuario (derecho de eliminacion PIPA).</summary>
        Task<ResultadoIdentity> EliminarCuentaAsync(string usuarioId);
    }
}
