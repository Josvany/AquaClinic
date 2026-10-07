namespace AquaClinic.Domain.Enums
{
    /// <summary>Estado de un token de restablecimiento de contrase&#241;a.</summary>
    public enum EstadoPasswordResetToken
    {
        /// <summary>Registrado, pendiente de ser consumido.</summary>
        Pendiente = 1,

        /// <summary>Consumido correctamente para restablecer la contrase&#241;a.</summary>
        Consumido = 2,

        /// <summary>Agotados los intentos o expirado sin consumirse.</summary>
        Vencido = 3
    }
}
