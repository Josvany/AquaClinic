using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
    {
        public void Configure(EntityTypeBuilder<Servicio> builder)
        {
            builder.Property(s => s.ServicioId).ValueGeneratedNever();
            builder.Property(s => s.Nombre).IsRequired().HasMaxLength(50);
            builder.Property(s => s.Descripcion).HasMaxLength(500);
            builder.Property(s => s.Precio).HasPrecision(10, 2);

            builder.HasOne(s => s.Especialidad)
                .WithMany(e => e.Servicios)
                .HasForeignKey(s => s.EspecialidadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.EspecialidadId);
        }
    }
}
