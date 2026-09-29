using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Models.Mail;
using AquaClinic.Infrastructure.Contabilidad;
using AquaClinic.Infrastructure.FileExport;
using AquaClinic.Infrastructure.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AquaClinic.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ICsvExporter, CsvExporter>();

            // Reintentos de correos de cita en segundo plano
            services.AddHostedService<EmailRetryBackgroundService>();

            // Mini-contabilidad de servicios atendidos en segundo plano
            services.AddHostedService<ContabilidadBackgroundService>();

            return services;
        }
    }
}
