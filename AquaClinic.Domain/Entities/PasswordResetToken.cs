using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Token de restablecimiento de contrase&#241;a generado para un correo.
    /// Almacena el token emitido por Identity, su estado de consumo y la
    /// trazabilidad de intentos y vencimiento para garantizar un solo uso
    /// real (no s&#243; conf&#237;anza en el generador de tokens de Identity).
    /// </summary>
    public class PasswordResetToken : AuditableEntity
    {
        public PasswordResetToken()
        {
            CreadoUtc = DateTime.UtcNow;
            Intentos = 0;
            Consumido = false;
            Vencido = false;
        }

        /// <summary>Correo del usuario al que corresponde el token.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Valor generado por UserManager.GeneratePasswordResetTokenAsync.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Indica si el token ya fue consumido para restablecer la contrase&#241;a.</summary>
        public bool Consumido { get; set; }

        /// <summary>Cantidad de intentos de restablecimiento realizados con este token.</summary>
        public int Intentos { get; set; }

        /// <summary>Indica si el token venc&#243; sin consumirse.</summary>
        public bool Vencido { get; set; }

        /// <summary>Hora de creaci&#243;n del token en UTC (usada como referencia de expiraci&#243;n).</summary>
        public DateTime CreadoUtc { get; set; }

        /// <summary>
        /// Hora de expiraci&#243;n del token. Se ha de mantener la compatibilidad con la
        /// expiraci&#243;n de Identity: si se deja null, el timeout se decide por el
        /// proveedor de tokens de Identity (IdentityOptions.PasswordResetTokenProvider
        /// / UserManager); se recomienda definir un valor expl&#237;cito una vez que
        /// quede documentado c&#243;mo y d&#243;nde se configura.
        /// </summary>
        public DateTime? Expira { get; set; }
    }
}
