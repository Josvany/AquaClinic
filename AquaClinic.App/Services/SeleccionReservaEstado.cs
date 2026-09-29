using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// Estado compartido (scoped) entre la pagina /reservar y el ReservaDialog.
    /// Guarda el servicio elegido en la pagina para que el modal lo tenga
    /// preseleccionado y permita re-anclar la reserva a otra hora cuando el
    /// servicio no cabe en el slot inicial (sugerencias de horas alternas).
    /// </summary>
    public class SeleccionReservaEstado
    {
        /// <summary>Servicio elegido antes de abrir el calendario (null = sin elegir).</summary>
        public ServicioPublicoVm? ServicioElegido { get; private set; }

        /// <summary>
        /// Servicio sugerido por id (ej. desde un banner del index via query string).
        /// El modal lo resuelve contra el catalogo; si el id no existe se ignora.
        /// </summary>
        public Guid? ServicioSugeridoId { get; private set; }

        /// <summary>Nueva hora de inicio elegida desde las sugerencias del modal.</summary>
        public DateTime? NuevoInicioLocal { get; private set; }

        public void EstablecerServicio(ServicioPublicoVm? servicio)
        {
            ServicioElegido = servicio;
            ServicioSugeridoId = null;
            NuevoInicioLocal = null;
        }

        /// <summary>Sugiere un servicio solo por id (sin VM; el modal lo resuelve).</summary>
        public void EstablecerServicioSugerido(Guid servicioId)
        {
            ServicioSugeridoId = servicioId;
            ServicioElegido = null;
            NuevoInicioLocal = null;
        }

        /// <summary>El modal pide mover la reserva a otra hora.</summary>
        public void ReAnclar(DateTime nuevoInicioLocal)
        {
            NuevoInicioLocal = nuevoInicioLocal;
        }

        public void Reiniciar()
        {
            NuevoInicioLocal = null;
        }
    }
}
