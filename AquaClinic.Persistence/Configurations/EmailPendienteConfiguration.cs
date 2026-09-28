using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class EmailPendienteConfiguration : IEntityTypeConfiguration<EmailPendiente>
    {
        public void Configure(EntityTypeBuilder<EmailPendiente> builder)
        {
            builder.Property(e => e.EmailPendienteId).ValueGeneratedNever();
            builder.Property(e => e.Destinatario).HasMaxLength(256);
            builder.Property(e => e.Asunto).HasMaxLength(200);
            builder.Property(e => e.Cuerpo).HasMaxLength(4000);
            builder.Property(e => e.UltimoError).HasMaxLength(1000);
            builder.Property(e => e.Estado).HasConversion<int>();

            // Consulta del reintento: pendientes ordenados por proximo intento
            builder.HasIndex(e => new { e.Estado, e.UltimoIntentoUtc });

            // Trazabilidad con la cita (opcional)
            builder.HasOne<Cita>()
                .WithMany()
                .HasForeignKey(e => e.CitaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
