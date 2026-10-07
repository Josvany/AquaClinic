using AquaClinic.App.Localization;
using AquaClinic.App.Services.Base;

namespace AquaClinic.App.ViewModels
{
    /// <summary>Servicio del catalogo visible en el modal de reserva.</summary>
    public class ServicioPublicoVm
    {
        public Guid ServicioId { get; set; }

        /// <summary>Nombre puro del servicio (sin precio/duracion concatenados).</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Descripcion del tratamiento (para las tarjetas de la portada).</summary>
        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }

        /// <summary>Nombre de la especialidad a la que pertenece.</summary>
        public string? EspecialidadNombre { get; set; }

        /// <summary>Etiqueta compuesta para el dropdown del modal: "Nombre — $XX (Y min)".</summary>
        public string EtiquetaDropdown => $"{Nombre} — {Precio.ToString("C", FormatoClinica.CulturaDinero)} ({DuracionMinutos} min)";

        /// <summary>Promocion vigente que aplica al servicio (null si ninguna).</summary>
        public string? NombrePromocion { get; set; }

        /// <summary>1 = porcentaje, 2 = monto fijo en CAD.</summary>
        public int? TipoDescuentoPromocion { get; set; }

        /// <summary>Porcentaje (15.00 = 15%) o monto en CAD, segun TipoDescuentoPromocion.</summary>
        public decimal? ValorPromocion { get; set; }

        /// <summary>True cuando el servicio tiene una promocion vigente en el catalogo.</summary>
        public bool TienePromocion => ValorPromocion.HasValue;

        /// <summary>Texto del descuento para las tarjetas: "15%" o "$20 CAD".</summary>
        public string DescuentoTexto => TipoDescuentoPromocion == 2
            ? string.Format(TextoUi.T("-{0} CAD"), ValorPromocion!.Value.ToString("0.00", FormatoClinica.CulturaDinero))
            : string.Format(TextoUi.T("-{0}%"), ValorPromocion!.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));

        /// <summary>Precio tras el descuento (sin promo devuelve el precio original).</summary>
        public decimal PrecioConDescuento => ValorPromocion is not decimal valor
            ? Precio
            : Math.Max(0, decimal.Round(
                TipoDescuentoPromocion == 2 ? Precio - valor : Precio * (1 - valor / 100m), 2));
    }

    /// <summary>Datos que el modal envia para crear la cita.</summary>
    public class ReservaCitaVm
    {
        public Guid ServicioId { get; set; }
        public DateTime FechaInicioLocal { get; set; }
        public int? DuracionMinutos { get; set; }
        public string? UsuarioId { get; set; }
        public string? NombreInvitado { get; set; }
        public string? EmailInvitado { get; set; }
        public string? TelefonoInvitado { get; set; }
        public string? Notas { get; set; }
    }
}

namespace AquaClinic.App.Contracts
{
using AquaClinic.App.Services.Base;
    using AquaClinic.App.ViewModels;

    /// <summary>Catalogo publico de servicios (sin sesion).</summary>
    public interface IServiciosPublicosDataService
    {
        Task<IEnumerable<ServicioPublicoVm>> ObtenerServiciosAsync();
    }

    /// <summary>Creacion de citas desde el scheduler publico.</summary>
    public interface IReservasDataService
    {
        Task<ApiResponse<Guid>> CrearCitaAsync(ReservaCitaVm reserva);
    }
}
