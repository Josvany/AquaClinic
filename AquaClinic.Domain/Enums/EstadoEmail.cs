namespace AquaClinic.Domain.Enums
{
    /// <summary>
    /// Estado de un correo de la cola de salida (outbox).
    /// </summary>
    public enum EstadoEmail
    {
        /// <summary>Registrado, pendiente de envio (o de reintento).</summary>
        Pendiente = 1,

        /// <summary>Entregado al proveedor de correo; estado final.</summary>
        Enviado = 2,

        /// <summary>Agoto los reintentos sin confirmarse; estado final auditable.</summary>
        Fallido = 3
    }
}
