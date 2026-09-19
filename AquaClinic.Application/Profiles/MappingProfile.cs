using AutoMapper;
using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithCitas;
using AquaClinic.Application.Features.Citas.Commands.CreateCita;
using AquaClinic.Application.Features.Citas.Commands.UpdateCita;
using AquaClinic.Application.Features.Citas.Queries.GetCitaDetail;
using AquaClinic.Application.Features.Citas.Queries.GetCitasExport;
using AquaClinic.Application.Features.Citas.Queries.GetCitasList;
using AquaClinic.Application.Features.Consultas.Queries.GetConsultasForMonth;
using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Cita, CitaListVm>().ReverseMap();
            CreateMap<Cita, CitaDetailVm>().ReverseMap();
            CreateMap<Especialidad, EspecialidadDto>().ReverseMap();
            CreateMap<Especialidad, EspecialidadListVm>().ReverseMap();
            CreateMap<Especialidad, EspecialidadCitaListVm>().ReverseMap();
            CreateMap<Especialidad, CreateEspecialidadCommand>().ReverseMap();
            CreateMap<Especialidad, CreateEspecialidadDto>().ReverseMap();
            CreateMap<Cita, EspecialidadCitaDto>().ReverseMap();
            CreateMap<Cita, CitaExportDto>().ReverseMap();

            CreateMap<Cita, CreateCitaCommand>().ReverseMap();
            CreateMap<Cita, UpdateCitaCommand>().ReverseMap();
            CreateMap<Cita, EspecialidadCitaDto>().ReverseMap();

            CreateMap<Consulta, ConsultasForMonthDto>();
        }
    }
}