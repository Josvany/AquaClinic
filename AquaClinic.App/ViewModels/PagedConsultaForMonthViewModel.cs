namespace AquaClinic.App.ViewModels
{
    public class PagedConsultaForMonthViewModel
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public ICollection<ConsultasForMonthListViewModel>? ConsultasForMonth { get; set; }
    }
}
