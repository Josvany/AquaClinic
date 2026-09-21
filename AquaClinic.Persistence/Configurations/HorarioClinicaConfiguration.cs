using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class HorarioClinicaConfiguration : IEntityTypeConfiguration<HorarioClinica>
    {
        public void Configure(EntityTypeBuilder<HorarioClinica> builder)
        {
            builder.HasKey(h => h.HorarioId);
            builder.Property(h => h.HorarioId).ValueGeneratedNever();

            // Un unico horario por dia de la semana
            builder.HasIndex(h => h.DiaSemana).IsUnique();
        }
    }
}
