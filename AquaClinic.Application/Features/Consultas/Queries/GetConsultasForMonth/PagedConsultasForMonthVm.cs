namespace AquaClinic.Application.Features.Consultas.Queries.GetConsultasForMonth
{
    public class PagedConsultasForMonthVm
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public ICollection<ConsultasForMonthDto>? ConsultasForMonth { get; set; }
    }
}