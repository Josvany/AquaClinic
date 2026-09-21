using AquaClinic.Domain.Entities;
using AquaClinic.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class PersonalConfiguration : IEntityTypeConfiguration<Personal>
    {
        public void Configure(EntityTypeBuilder<Personal> builder)
        {
            builder.Property(p => p.PersonalId).ValueGeneratedNever();
            builder.Property(p => p.Nombre).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Apellidos).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Telefono).HasMaxLength(30);
            builder.Property(p => p.Email).HasMaxLength(256);

            // Cuenta de login vinculada: única cuando está asignada (varios null permitidos)
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.ApplicationUserId)
                .IsUnique()
                .HasFilter("[ApplicationUserId] IS NOT NULL");
        }
    }
}
