using AquaClinic.App.Services.Base;

namespace AquaClinic.App.ViewModels
{
    /// <summary>Servicio del catalogo visible en el modal de reserva.</summary>
    public class ServicioPublicoVm
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
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
