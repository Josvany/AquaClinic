using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class MovimientoServicioConfiguration : IEntityTypeConfiguration<MovimientoServicio>
    {
        public void Configure(EntityTypeBuilder<MovimientoServicio> builder)
        {
            builder.HasKey(m => m.MovimientoServicioId);
            builder.Property(m => m.MovimientoServicioId).ValueGeneratedNever();

            builder.Property(m => m.NombreCliente).HasMaxLength(200);
            builder.Property(m => m.UsuarioId).HasMaxLength(450);
            builder.Property(m => m.PrecioSnapshot).HasPrecision(10, 2);

            // La contabilidad es un registro historico: no se borra en cascada
            // si desaparece la cita o el servicio.
            builder.HasOne(m => m.Cita)
                .WithMany()
                .HasForeignKey(m => m.CitaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Servicio)
                .WithMany()
                .HasForeignKey(m => m.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Personal>()
                .WithMany()
                .HasForeignKey(m => m.PersonalId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<AquaClinic.Identity.Models.ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Idempotencia del worker: una cita nunca se cuenta dos veces
            builder.HasIndex(m => new { m.CitaId, m.ServicioId }).IsUnique();

            // Consultas por rango de fechas (conteo diario/mensual)
            builder.HasIndex(m => m.FechaAtencion);
        }
    }
}
