using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.MoverMisCita
{
    /// <summary>
    /// Reprograma una cita del propio usuario autenticado (pantalla
    /// /miscitas). A diferencia de MoverCitaCommand (uso interno), aqui el
    /// cliente solo puede mover citas que le pertenecen: el handler valida la
    /// propiedad antes de delegar en la logica de disponibilidad.
    /// El nuevo inicio llega en hora local de la clinica (America/Vancouver).
    /// </summary>
    public class MoverMisCitaCommand : IRequest<Guid>
    {
        public Guid CitaId { get; set; }

        /// <summary>Nuevo inicio en hora local de la clinica (America/Vancouver).</summary>
        public DateTime NuevoInicioLocal { get; set; }

        /// <summary>Duracion en minutos; si es null se usa la del servicio de la cita.</summary>
        public int? DuracionMinutos { get; set; }
    }
}