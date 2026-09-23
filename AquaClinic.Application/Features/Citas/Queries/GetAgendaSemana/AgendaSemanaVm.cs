namespace AquaClinic.Application.Features.Citas.Queries.GetAgendaSemana
{
    /// <summary>Bloque ocupado/reservando del scheduler publico (sin datos de clientes).</summary>
    public class OcupacionVm
    {
        public DateTimeOffset FechaInicio { get; set; }

        public DateTimeOffset FechaFin { get; set; }

        /// <summary>"Ocupado" (cita existente) o "Reservando" (reserva suave en curso).</summary>
        public string Estado { get; set; } = "Ocupado";
    }

    /// <summary>Agenda semanal publica: lista de bloques ocupados/reservando.</summary>
    public class AgendaSemanaVm
    {
        public List<OcupacionVm> Ocupaciones { get; set; } = [];
    }
}
