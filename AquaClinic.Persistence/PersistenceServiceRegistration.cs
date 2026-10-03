using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AquaClinic.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AquaClinicDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AquaClinicConnectionString")));

            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IServicioRepository, ServicioRepository>();
            services.AddScoped<IEspecialidadRepository, EspecialidadRepository>();
            services.AddScoped<ICitaRepository, CitaRepository>();
            services.AddScoped<IAgendaRepository, AgendaRepository>();
            services.AddScoped<IBloqueoRepository, BloqueoRepository>();
            services.AddScoped<IEmailPendienteRepository, EmailPendienteRepository>();
            services.AddScoped<IMovimientoServicioRepository, MovimientoServicioRepository>();

            return services;
        }
    }
}
