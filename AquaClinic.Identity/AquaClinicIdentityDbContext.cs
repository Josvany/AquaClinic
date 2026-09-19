using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AquaClinic.Identity
{
    public class AquaClinicIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public AquaClinicIdentityDbContext()
        {

        }

        public AquaClinicIdentityDbContext(DbContextOptions<AquaClinicIdentityDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
        .LogTo(Console.WriteLine)
        .EnableSensitiveDataLogging();

    }
}
