using AquaClinic.Domain.Entities;
using AquaClinic.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class MovimientoPuntosConfiguration : IEntityTypeConfiguration<MovimientoPuntos>
    {
        public void Configure(EntityTypeBuilder<MovimientoPuntos> builder)
        {
            builder.HasKey(m => m.MovimientoId);
            builder.Property(m => m.MovimientoId).ValueGeneratedNever();
            builder.Property(m => m.UsuarioId).IsRequired().HasMaxLength(450);
            builder.Property(m => m.Motivo).IsRequired().HasMaxLength(100);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Cita>()
                .WithMany()
                .HasForeignKey(m => m.CitaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(m => m.UsuarioId);
        }
    }
}
