using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Consultas.Queries.GetConsultasForMonth
{
    public class GetConsultasForMonthQueryHandler : IRequestHandler<GetConsultasForMonthQuery, PagedConsultasForMonthVm>
    {
        private readonly IConsultaRepository _consultaRepository;
        private readonly IMapper _mapper;

        public GetConsultasForMonthQueryHandler(IConsultaRepository consultaRepository, IMapper mapper)
        {
            _consultaRepository = consultaRepository;
            _mapper = mapper;
        }

        public async Task<PagedConsultasForMonthVm> Handle(GetConsultasForMonthQuery request, CancellationToken cancellationToken)
        {
            var list = await _consultaRepository.GetPagedConsultasForMonth(request.Fecha, request.Page, request.Size);
            var orders = _mapper.Map<List<ConsultasForMonthDto>>(list);

            var count = await _consultaRepository.GetTotalCountOfConsultasForMonth(request.Fecha);
            return new PagedConsultasForMonthVm() { Count = count, ConsultasForMonth = orders, Page = request.Page, Size = request.Size };
        }
    }
}
