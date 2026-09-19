using MediatR;

namespace AquaClinic.Application.Features.Consultas.Queries.GetConsultasForMonth
{
    public class GetConsultasForMonthQuery : IRequest<PagedConsultasForMonthVm>
    {
        public DateTime Fecha { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
