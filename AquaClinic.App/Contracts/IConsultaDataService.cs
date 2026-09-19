using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Contracts
{
    public interface IConsultaDataService
    {
        Task<PagedConsultaForMonthViewModel> GetPagedConsultaForMonth(DateTime date, int page, int size);
    }
}
