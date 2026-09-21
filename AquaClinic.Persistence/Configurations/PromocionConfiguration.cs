using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
    {
        public void Configure(EntityTypeBuilder<Promocion> builder)
        {
            builder.Property(p => p.PromocionId).ValueGeneratedNever();
            builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Descripcion).HasMaxLength(500);
            builder.Property(p => p.Valor).HasPrecision(10, 2);
        }
    }
}
