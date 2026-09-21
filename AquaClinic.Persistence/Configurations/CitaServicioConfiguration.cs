using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class CitaServicioConfiguration : IEntityTypeConfiguration<CitaServicio>
    {
        public void Configure(EntityTypeBuilder<CitaServicio> builder)
        {
            builder.HasKey(cs => new { cs.CitaId, cs.ServicioId });

            builder.HasOne(cs => cs.Cita)
                .WithMany(c => c.CitasServicios)
                .HasForeignKey(cs => cs.CitaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cs => cs.Servicio)
                .WithMany()
                .HasForeignKey(cs => cs.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
