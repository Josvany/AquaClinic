using AquaClinic.Domain.Entities;
using AquaClinic.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class CitaConfiguration : IEntityTypeConfiguration<Cita>
    {
        public void Configure(EntityTypeBuilder<Cita> builder)
        {
            builder.Property(c => c.CitaId).ValueGeneratedNever();
            builder.Property(c => c.NombreInvitado).HasMaxLength(100);
            builder.Property(c => c.EmailInvitado).HasMaxLength(256);
            builder.Property(c => c.TelefonoInvitado).HasMaxLength(30);
            builder.Property(c => c.Notas).HasMaxLength(500);

            // Cliente registrado (opcional; null = invitado)
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Personal asignado (opcional)
            builder.HasOne<Personal>()
                .WithMany()
                .HasForeignKey(c => c.PersonalId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(c => c.FechaInicio);
            builder.HasIndex(c => c.UsuarioId);
        }
    }
}
