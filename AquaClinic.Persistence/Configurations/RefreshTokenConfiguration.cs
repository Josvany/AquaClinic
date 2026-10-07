using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    /// <summary>
    /// Refresh tokens de la API. Solo se guarda el SHA-256 del token, nunca el
    /// token en claro (ver RefreshToken).
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(t => t.RefreshTokenId);

            // Base64 del SHA-256: 44 caracteres con relleno.
            builder.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(t => t.UsuarioId)
                .IsRequired()
                .HasMaxLength(450);

            // La validacion busca por hash una sola vez; el indice unico la hace
            // O(1) y además impide dos filas con el mismo token.
            builder.HasIndex(t => t.TokenHash)
                .IsUnique();

            // Purga de vencidos y consulta de "mis tokens".
            builder.HasIndex(t => t.UsuarioId);
            builder.HasIndex(t => t.ExpiraUtc);
        }
    }
}