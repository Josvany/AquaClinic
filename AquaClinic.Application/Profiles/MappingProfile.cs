using AutoMapper;
using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithServicios;
using AquaClinic.Application.Features.Servicios.Commands.CreateServicio;
using AquaClinic.Application.Features.Servicios.Commands.UpdateServicio;
using AquaClinic.Application.Features.Servicios.Queries.GetServicioDetail;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosExport;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosList;
using AquaClinic.Application.Features.Usuarios;
using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Servicio, ServicioListVm>().ReverseMap();
            CreateMap<Servicio, ServicioDetailVm>().ReverseMap();
            CreateMap<Especialidad, EspecialidadDto>().ReverseMap();
            CreateMap<Especialidad, EspecialidadListVm>().ReverseMap();
            CreateMap<Especialidad, EspecialidadServicioListVm>().ReverseMap();
            CreateMap<Especialidad, CreateEspecialidadCommand>().ReverseMap();
            CreateMap<Especialidad, CreateEspecialidadDto>().ReverseMap();
            CreateMap<Servicio, EspecialidadServicioDto>().ReverseMap();
            CreateMap<Servicio, ServicioExportDto>().ReverseMap();

            CreateMap<Servicio, CreateServicioCommand>().ReverseMap();
            CreateMap<Servicio, UpdateServicioCommand>().ReverseMap();

            // Usuarios (perfil del cliente)
            CreateMap<Usuario, UsuarioVm>();
        }
    }
}
