using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class PromocionServicioConfiguration : IEntityTypeConfiguration<PromocionServicio>
    {
        public void Configure(EntityTypeBuilder<PromocionServicio> builder)
        {
            builder.HasKey(ps => new { ps.PromocionId, ps.ServicioId });

            builder.HasOne(ps => ps.Promocion)
                .WithMany(p => p.PromocionesServicios)
                .HasForeignKey(ps => ps.PromocionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ps => ps.Servicio)
                .WithMany()
                .HasForeignKey(ps => ps.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
