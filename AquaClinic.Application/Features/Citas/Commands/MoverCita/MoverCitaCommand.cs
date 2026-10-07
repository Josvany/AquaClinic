using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.MoverCita
{
    /// <summary>
    /// Mueve una cita pendiente o confirmada a otro horario. El nuevo
    /// inicio llega en hora local de la clinica (America/Vancouver); la
    /// duracion por defecto es la del servicio vinculado a la cita.
    /// </summary>
    public class MoverCitaCommand : IRequest<Guid>
    {
        public Guid CitaId { get; set; }

        /// <summary>Nuevo inicio en hora local de la clinica (America/Vancouver).</summary>
        public DateTime NuevoInicioLocal { get; set; }

        /// <summary>Duracion en minutos; si es null se usa la del servicio de la cita.</summary>
        public int? DuracionMinutos { get; set; }
    }
}
