using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitaById
{
    /// <summary>
    /// Detalle de una cita (por Id) para la pantalla /citadetails.
    /// </summary>
    public class GetCitaByIdQuery : IRequest<CitaDetailVm>
    {
        public Guid Id { get; set; }
    }
}
