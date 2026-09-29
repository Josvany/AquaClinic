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
        public string EtiquetaDropdown => $"{Nombre} — {Precio.ToString("C", new System.Globalization.CultureInfo("en-CA"))} ({DuracionMinutos} min)";
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
