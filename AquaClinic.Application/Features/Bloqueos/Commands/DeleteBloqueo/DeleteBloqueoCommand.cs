using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Commands.DeleteBloqueo
{
    /// <summary>
    /// Elimina un bloqueo: al no tener historial asociado (las citas no lo
    /// referencian) el borrado es fisico y el rango vuelve a estar disponible.
    /// </summary>
    public class DeleteBloqueoCommand : IRequest
    {
        public Guid BloqueoId { get; set; }
    }
}
