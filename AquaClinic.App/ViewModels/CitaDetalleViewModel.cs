namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Detalle real de una cita (API GET api/citas/{id}): cliente
    /// (registrado o invitado), servicio(s) contratado(s), fecha/hora
    /// local de la clinica, estado, notas y personal asignado.
    /// </summary>
    public class CitaDetalleViewModel
    {
        public Guid CitaId { get; set; }

        /// <summary>True cuando el cliente es usuario registrado.</summary>
        public bool EsRegistrado { get; set; }

        /// <summary>Nombre visible del cliente.</summary>
        public string Cliente { get; set; } = string.Empty;

        public string? EmailContacto { get; set; }

        public string? TelefonoContacto { get; set; }

        /// <summary>Personal asignado; null = por asignar.</summary>
        public string? PersonalNombre { get; set; }

        /// <summary>Inicio en hora local de la clinica.</summary>
        public DateTime FechaInicioLocal { get; set; }

        /// <summary>Fin en hora local de la clinica.</summary>
        public DateTime FechaFinLocal { get; set; }

        /// <summary>Estado legible (Pendiente/Confirmada/Cancelada/Completada).</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Valor numerico del estado (1=Pendiente, 2=Confirmada).</summary>
        public int EstadoValor { get; set; }

        public string? Notas { get; set; }

        public List<CitaDetalleServicioViewModel> Servicios { get; set; } = new();

        /// <summary>Duracion de la cita segun el primer servicio (60 min por defecto).</summary>
        public int DuracionMinutos =>
            Servicios.Count > 0 ? Servicios[0].DuracionMinutos : 60;

        /// <summary>Solo citas pendientes o confirmadas se pueden mover o cancelar.</summary>
        public bool PermiteGestion => EstadoValor is 1 or 2;
    }

    public class CitaDetalleServicioViewModel
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public string? EspecialidadNombre { get; set; }
    }
}
