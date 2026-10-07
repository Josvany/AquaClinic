using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    /// <summary>
    /// Token de restablecimiento: clave compuesta (Email + Token). El token NO es
    /// columna calculada (EF no permite valor generada sobre una clave); el
    /// hasheo se hace en el codigo de la aplicacion antes de guardar.
    /// </summary>
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(pt => new { pt.Email, pt.Token });

            builder.Property(pt => pt.Email)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(pt => pt.Token)
                .HasMaxLength(450)
                .IsRequired();

            builder.HasIndex(pt => pt.Email);

            builder.Property(pt => pt.Expira)
                .HasColumnName("Expira");
        }
    }
}
