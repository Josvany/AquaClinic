using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasList
{
    public class GetCitasListQueryHandler : IRequestHandler<GetCitasListQuery, List<CitaListVm>>
    {
        private readonly IAsyncRepository<Cita> _citaRepository;
        private readonly IMapper _mapper;

        public GetCitasListQueryHandler(IMapper mapper, IAsyncRepository<Cita> citaRepository)
        {
            _mapper = mapper;
            _citaRepository = citaRepository;
        }

        public async Task<List<CitaListVm>> Handle(GetCitasListQuery request, CancellationToken cancellationToken)
        {
            var allCitas = (await _citaRepository.ListAllAsync()).OrderBy(x => x.Fecha);
            return _mapper.Map<List<CitaListVm>>(allCitas);
        }
    }
}
