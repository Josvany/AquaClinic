namespace AquaClinic.Application.Features.Consultas.Queries.GetConsultasForMonth
{
    public class ConsultasForMonthDto
    {
        public Guid Id { get; set; }
        public int TotalConsulta { get; set; }
        public DateTime FechaSolicitada { get; set; }
    }
}