using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(pt => new { pt.Email, pt.Token });

            builder.Property(pt => pt.Email)
                .HasMaxLength(256)
                .IsRequired();

            // El token se almacena hasheado (SHA2_256) por quien lo persiste,
            // no como columna calculada: una propiedad parte de la clave no
            // puede ser ValueGeneratedOnAddOrUpdate en EF Core, y SQL Server
            // tampoco admite una clave primaria sobre columna calculada no
            // persistida. La consulta por Email + Token compara el hash.
            builder.Property(pt => pt.Token)
                .HasMaxLength(450)
                .IsRequired();

            builder.HasIndex(pt => pt.Email);

            builder.Property(pt => pt.Consumido)
                .HasColumnName("Consumido");

            builder.Property(pt => pt.Intentos)
                .HasColumnName("Intentos");

            builder.Property(pt => pt.Vencido)
                .HasColumnName("Vencido");

            builder.Property(pt => pt.CreadoUtc)
                .HasColumnName("CreadoUtc");

            builder.Property(pt => pt.Expira)
                .HasColumnName("Expira");
        }
    }
}
