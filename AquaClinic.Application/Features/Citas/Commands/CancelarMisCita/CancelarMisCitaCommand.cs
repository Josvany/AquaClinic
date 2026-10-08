using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.CancelarMisCita
{
    /// <summary>
    /// Cancela una cita del propio usuario autenticado (pantalla
    /// /miscitas). El handler valida que la cita le pertenezca antes de
    /// delegar en la logica de cancelacion.
    /// </summary>
    public class CancelarMisCitaCommand : IRequest
    {
        public Guid CitaId { get; set; }
    }
}