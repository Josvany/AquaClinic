using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitaDetail
{
    public class GetCitaDetailQueryHandler : IRequestHandler<GetCitaDetailQuery, CitaDetailVm>
    {
        private readonly IAsyncRepository<Cita> _citaRepository;
        private readonly IAsyncRepository<Especialidad> _especialidadRepository;
        private readonly IMapper _mapper;

        public GetCitaDetailQueryHandler(
            IMapper mapper,
            IAsyncRepository<Cita> citaRepository,
            IAsyncRepository<Especialidad> especialidadRepository)
        {
            _mapper = mapper;
            _citaRepository = citaRepository;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<CitaDetailVm> Handle(GetCitaDetailQuery request, CancellationToken cancellationToken)
        {
            var @cita = await _citaRepository.GetByIdAsync(request.Id);
            var citaDetailDto = _mapper.Map<CitaDetailVm>(@cita);

            var category = await _especialidadRepository.GetByIdAsync(@cita.EspecialidadId);

            citaDetailDto.Especialidad = _mapper.Map<EspecialidadDto>(category);

            return citaDetailDto;
        }
    }
}
