using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class BloqueoConfiguration : IEntityTypeConfiguration<Bloqueo>
    {
        public void Configure(EntityTypeBuilder<Bloqueo> builder)
        {
            builder.Property(b => b.BloqueoId).ValueGeneratedNever();
            builder.Property(b => b.Motivo).IsRequired().HasMaxLength(100);

            builder.HasOne(b => b.Personal)
                .WithMany()
                .HasForeignKey(b => b.PersonalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => b.FechaInicio);
        }
    }
}
