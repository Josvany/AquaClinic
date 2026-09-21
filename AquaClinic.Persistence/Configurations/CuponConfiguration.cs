using AquaClinic.Domain.Entities;
using AquaClinic.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class CuponConfiguration : IEntityTypeConfiguration<Cupon>
    {
        public void Configure(EntityTypeBuilder<Cupon> builder)
        {
            builder.Property(c => c.CuponId).ValueGeneratedNever();
            builder.Property(c => c.Codigo).HasMaxLength(20);

            builder.HasOne(c => c.Promocion)
                .WithMany()
                .HasForeignKey(c => c.PromocionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente al que se asigno el cupon (opcional)
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Codigo unico cuando esta definido (varios null permitidos)
            builder.HasIndex(c => c.Codigo)
                .IsUnique()
                .HasFilter("[Codigo] IS NOT NULL");
        }
    }
}
