using AutoMapper;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services.Base;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Services
{
    public class EspecialidadDataService : BaseDataService, IEspecialidadDataService
    {
        private readonly IMapper _mapper;

        public EspecialidadDataService(IClient client, IMapper mapper): base(client)
        {
            _mapper = mapper;
        }

        public async Task<List<EspecialidadViewModel>> GetAllEspecialidades()
        {
            var allEspecialidades = await _client.GetAllEspecialidadesAsync();
            var mappedEspecialidades = _mapper.Map<ICollection<EspecialidadViewModel>>(allEspecialidades);
            return mappedEspecialidades.ToList();

        }

        public async Task<List<EspecialidadCitasViewModel>> GetAllEspecialidadesWithCitas(bool incluirHistorial)
        {
            var allEspecialidades = await _client.GetEspecialidadesWithCitasAsync(incluirHistorial);
            var mappedEspecialidades = _mapper.Map<ICollection<EspecialidadCitasViewModel>>(allEspecialidades);
            return mappedEspecialidades.ToList();
        }

        public async Task<ApiResponse<EspecialidadDto>> CreateEspecialidad(EspecialidadViewModel especialidadViewModel)
        {
            try
            {
                ApiResponse<EspecialidadDto> apiResponse = new ApiResponse<EspecialidadDto>();
                CreateEspecialidadCommand createEspecialidadCommand = _mapper.Map<CreateEspecialidadCommand>(especialidadViewModel);
                var createEspecialidadCommandResponse = await _client.AddEspecialidadAsync(createEspecialidadCommand);
                if (createEspecialidadCommandResponse.Success)
                {
                    apiResponse.Data = _mapper.Map<EspecialidadDto>(createEspecialidadCommandResponse.Especialidad);
                    apiResponse.Success = true;
                }
                else
                {
                    apiResponse.Data = null;
                    foreach (var error in createEspecialidadCommandResponse.ValidationErrors)
                    {
                        apiResponse.ValidationErrors += error + Environment.NewLine;
                    }
                }
                return apiResponse;
            }
            catch (ApiException ex)
            {
                return ConvertApiExceptions<EspecialidadDto>(ex);
            }
        }
    }
}