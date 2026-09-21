using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaClinic.Persistence.Configurations
{
    public class PersonalEspecialidadConfiguration : IEntityTypeConfiguration<PersonalEspecialidad>
    {
        public void Configure(EntityTypeBuilder<PersonalEspecialidad> builder)
        {
            builder.HasKey(pe => new { pe.PersonalId, pe.EspecialidadId });

            builder.HasOne(pe => pe.Personal)
                .WithMany(p => p.PersonalEspecialidades)
                .HasForeignKey(pe => pe.PersonalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pe => pe.Especialidad)
                .WithMany()
                .HasForeignKey(pe => pe.EspecialidadId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
