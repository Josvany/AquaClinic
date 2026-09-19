using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Contracts.Persistence
{
    public interface IConsultaRepository : IAsyncRepository<Consulta>
    {
        Task<List<Consulta>> GetPagedConsultasForMonth(DateTime date, int page, int size);
        Task<int> GetTotalCountOfConsultasForMonth(DateTime date);
    }
}
