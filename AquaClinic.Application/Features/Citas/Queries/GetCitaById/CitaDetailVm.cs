namespace AquaClinic.Application.Features.Citas.Queries.GetCitaById
{
    /// <summary>
    /// Detalle real de una cita para /citadetails: cliente (registrado
    /// o invitado), servicio(s) contratado(s), fecha/hora local de la
    /// clinica, estado, notas y personal asignado.
    /// </summary>
    public class CitaDetailVm
    {
        public Guid CitaId { get; set; }

        /// <summary>True cuando el cliente es usuario registrado (UsuarioId).</summary>
        public bool EsRegistrado { get; set; }

        /// <summary>Nombre visible del cliente (usuario registrado o invitado).</summary>
        public string Cliente { get; set; } = string.Empty;

        /// <summary>Correo de contacto (usuario registrado o invitado).</summary>
        public string? EmailContacto { get; set; }

        /// <summary>Telefono de contacto (solo invitado).</summary>
        public string? TelefonoContacto { get; set; }

        /// <summary>Personal asignado; null = por asignar.</summary>
        public string? PersonalNombre { get; set; }

        /// <summary>Inicio en hora local de la clinica (America/Vancouver).</summary>
        public DateTime FechaInicioLocal { get; set; }

        /// <summary>Fin en hora local de la clinica.</summary>
        public DateTime FechaFinLocal { get; set; }

        /// <summary>Estado legible (Pendiente/Confirmada/Cancelada/Completada).</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Valor numerico del estado para la logica de la UI.</summary>
        public int EstadoValor { get; set; }

        public string? Notas { get; set; }

        public List<CitaDetalleServicioDto> Servicios { get; set; } = new();
    }

    public class CitaDetalleServicioDto
    {
        public Guid ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public string? EspecialidadNombre { get; set; }
    }
}
