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

            CreateMap<PagedConsultasForMonthVm, PagedConsultaForMonthViewModel>().ReverseMap();
            CreateMap<ConsultasForMonthDto, ConsultasForMonthListViewModel>().ReverseMap();
        }
    }
}
