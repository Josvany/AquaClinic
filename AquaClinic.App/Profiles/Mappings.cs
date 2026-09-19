using AutoMapper;
using AquaClinic.App.Services;
using AquaClinic.App.ViewModels;

namespace AquaClinic.App.Profiles
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            //Vms are coming in from the API, ViewModel are the local entities in Blazor
            CreateMap<CitaListVm, CitaListViewModel>().ReverseMap();
            CreateMap<CitaDetailVm, CitaDetailViewModel>().ReverseMap();

            CreateMap<CitaDetailViewModel, CreateCitaCommand>().ReverseMap();
            CreateMap<CitaDetailViewModel, UpdateCitaCommand>().ReverseMap();

            CreateMap<EspecialidadCitaDto, CitaNestedViewModel>().ReverseMap();

            CreateMap<EspecialidadDto, EspecialidadViewModel>().ReverseMap();
            CreateMap<EspecialidadListVm, EspecialidadViewModel>().ReverseMap();
            CreateMap<EspecialidadCitaListVm, EspecialidadCitasViewModel>().ReverseMap();
            CreateMap<CreateEspecialidadCommand, EspecialidadViewModel>().ReverseMap();
            CreateMap<CreateEspecialidadDto, EspecialidadDto>().ReverseMap();

            CreateMap<PagedConsultasForMonthVm, PagedConsultaForMonthViewModel>().ReverseMap();
            CreateMap<ConsultasForMonthDto, ConsultasForMonthListViewModel>().ReverseMap();
        }
    }
}
