using Radzen;

namespace AquaClinic.App.Services
{
    /// <summary>Tipo de mensaje que se muestra al usuario.</summary>
    public enum TipoMensaje
    {
        Exito,
        Error,
        Advertencia,
        Informacion
    }

    /// <summary>
    /// Envoltura global del NotificationService de Radzen: muestra notificaciones
    /// emergentes (toast) en cualquier punto de la App sin acoplar cada componente
    /// a Radzen ni duplicar estilos. Requiere RadzenComponents en MainLayout.
    /// </summary>
    public class NotificadorUi
    {
        private readonly NotificationService _notificaciones;

        public NotificadorUi(NotificationService notificaciones)
        {
            _notificaciones = notificaciones;
        }

        public void Exito(string mensaje, string titulo = "Listo") =>
            Mostrar(mensaje, titulo, TipoMensaje.Exito);

        public void Error(string mensaje, string titulo = "Ocurrio un problema") =>
            Mostrar(mensaje, titulo, TipoMensaje.Error);

        public void Advertencia(string mensaje, string titulo = "Atencion") =>
            Mostrar(mensaje, titulo, TipoMensaje.Advertencia);

        public void Informacion(string mensaje, string titulo = "Aviso") =>
            Mostrar(mensaje, titulo, TipoMensaje.Informacion);

        public void Mostrar(string mensaje, string titulo, TipoMensaje tipo)
        {
            var (severidad, icono) = tipo switch
            {
                TipoMensaje.Exito => (NotificationSeverity.Success, "check_circle"),
                TipoMensaje.Error => (NotificationSeverity.Error, "error"),
                TipoMensaje.Advertencia => (NotificationSeverity.Warning, "warning"),
                _ => (NotificationSeverity.Info, "info")
            };

            _notificaciones.Notify(new NotificationMessage
            {
                Severity = severidad,
                Summary = titulo,
                Detail = mensaje,
                Duration = tipo == TipoMensaje.Error ? 6000 : 4000,
                Style = "font-family: var(--font-sans);",
                Payload = tipo
            });
        }
    }
}
