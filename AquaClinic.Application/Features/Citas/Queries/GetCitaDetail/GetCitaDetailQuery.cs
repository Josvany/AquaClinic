using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitaDetail
{
    public class GetCitaDetailQuery : IRequest<CitaDetailVm>
    {
        public Guid Id { get; set; }
    }
}
