using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class CitaConfiguration : IEntityTypeConfiguration<Cita>
    {
        public void Configure(EntityTypeBuilder<Cita> builder)
        {

            builder.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);


        }
    }
}
