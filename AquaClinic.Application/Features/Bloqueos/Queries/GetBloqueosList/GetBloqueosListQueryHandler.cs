using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Queries.GetBloqueosList
{
    public class GetBloqueosListQueryHandler : IRequestHandler<GetBloqueosListQuery, List<BloqueoListVm>>
    {
        private readonly IBloqueoRepository _bloqueoRepository;
        private readonly IMapper _mapper;

        public GetBloqueosListQueryHandler(IMapper mapper, IBloqueoRepository bloqueoRepository)
        {
            _mapper = mapper;
            _bloqueoRepository = bloqueoRepository;
        }

        public async Task<List<BloqueoListVm>> Handle(GetBloqueosListQuery request, CancellationToken cancellationToken)
        {
            var bloqueos = await _bloqueoRepository.GetBloqueosClinicaAsync();

            return _mapper.Map<List<BloqueoListVm>>(bloqueos);
        }
    }
}
