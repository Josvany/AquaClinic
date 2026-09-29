using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class ResumenDiarioServicioConfiguration : IEntityTypeConfiguration<ResumenDiarioServicio>
    {
        public void Configure(EntityTypeBuilder<ResumenDiarioServicio> builder)
        {
            builder.HasKey(r => r.ResumenDiarioId);
            builder.Property(r => r.ResumenDiarioId).ValueGeneratedNever();

            builder.Property(r => r.MontoTotal).HasPrecision(12, 2);

            // Un solo resumen por dia contable x servicio
            builder.HasIndex(r => new { r.FechaLocal, r.ServicioId }).IsUnique();

            builder.HasOne(r => r.Servicio)
                .WithMany()
                .HasForeignKey(r => r.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
