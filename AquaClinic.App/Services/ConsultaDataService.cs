using AutoMapper;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    public class ConsultaDataService : BaseDataService, IConsultaDataService
    {
        private readonly IMapper _mapper;

        public ConsultaDataService(IClient client, IMapper mapper) : base(client)
        {
            _mapper = mapper;
        }

        public async Task<PagedConsultaForMonthViewModel> GetPagedConsultaForMonth(DateTime date, int page, int size)
        {
            var orders = await _client.GetPagedConsultasForMonthAsync(date, page, size);
            var mappedOrders = _mapper.Map<PagedConsultaForMonthViewModel>(orders);
            return mappedOrders;
        }
    }
}
