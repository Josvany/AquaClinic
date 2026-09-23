using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.CreateCita
{
    /// <summary>
    /// Crea una cita desde el scheduler publico de reservas. El cliente puede ser
    /// invitado (datos de contacto obligatorios) o usuario registrado (UsuarioId).
    /// </summary>
    public class CreateCitaCommand : IRequest<Guid>
    {
        public Guid ServicioId { get; set; }

        /// <summary>Inicio de la cita en hora local de la clinica (America/Vancouver).</summary>
        public DateTime FechaInicioLocal { get; set; }

        /// <summary>Duracion en minutos; si es null se usa la del servicio.</summary>
        public int? DuracionMinutos { get; set; }

        /// <summary>Id del usuario registrado (null si el cliente es invitado).</summary>
        public string? UsuarioId { get; set; }

        public string? NombreInvitado { get; set; }
        public string? EmailInvitado { get; set; }
        public string? TelefonoInvitado { get; set; }

        public string? Notas { get; set; }

        public override string ToString()
        {
            return $"Cita: servicio {ServicioId}; inicio {FechaInicioLocal:yyyy-MM-dd HH:mm} (local)";
        }
    }
}
