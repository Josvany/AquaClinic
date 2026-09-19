using AquaClinic.Application.Contracts;
using AquaClinic.Domain.Common;
using AquaClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AquaClinic.Persistence
{
    public class AquaClinicDbContext : DbContext
    {
        private readonly ILoggedInUserService? _loggedInUserService;

        public AquaClinicDbContext(DbContextOptions<AquaClinicDbContext> options)
           : base(options)
        {
        }

        public AquaClinicDbContext(DbContextOptions<AquaClinicDbContext> options, ILoggedInUserService loggedInUserService)
            : base(options)
        {
            _loggedInUserService = loggedInUserService;
        }

        public DbSet<Cita> Citas { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Consulta> Consultas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AquaClinicDbContext).Assembly);

            //seed data, added through migrations
            var cardiologiaGuid = Guid.Parse("{B0788D2F-8003-43C1-92A4-EDC76A7C5DDE}");
            var pediatriaGuid = Guid.Parse("{6313179F-7837-473A-A4D5-A5571B43E6A6}");
            var dermatologiaGuid = Guid.Parse("{BF3F3002-7E53-441E-8B76-F6280BE284AA}");
            var traumatologiaGuid = Guid.Parse("{FE98F549-E790-4E9F-AA16-18C2292A2EE9}");

            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = cardiologiaGuid,
                Nombre = "Cardiología"
            });
            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = pediatriaGuid,
                Nombre = "Pediatría"
            });
            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = dermatologiaGuid,
                Nombre = "Dermatología"
            });
            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = traumatologiaGuid,
                Nombre = "Traumatología"
            });

            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = Guid.Parse("{EE272F8B-6096-4CB6-8625-BB4BB2D89E8B}"),
                Nombre = "Consulta de cardiología general",
                Precio = 65,
                Doctor = "Dr. Juan Pérez",
                Fecha = new DateTime(2027, 3, 15, 9, 0, 0),
                Descripcion = "Evaluación cardiológica completa que incluye revisión de historial clínico, medición de presión arterial y auscultación cardíaca.",
                ImagenUrl = "https://picsum.photos/seed/aquaclinic1/300/200",
                EspecialidadId = cardiologiaGuid
            });

            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = Guid.Parse("{3448D5A4-0F72-4DD7-BF15-C14A46B26C00}"),
                Nombre = "Control de hipertensión arterial",
                Precio = 85,
                Doctor = "Dra. María González",
                Fecha = new DateTime(2027, 6, 20, 10, 30, 0),
                Descripcion = "Seguimiento de pacientes con hipertensión: control de valores, ajuste de medicación y recomendaciones de estilo de vida.",
                ImagenUrl = "https://picsum.photos/seed/aquaclinic2/300/200",
                EspecialidadId = cardiologiaGuid
            });

            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = Guid.Parse("{B419A7CA-3321-4F38-BE8E-4D7B6A529319}"),
                Nombre = "Electrocardiograma y evaluación cardíaca",
                Precio = 85,
                Doctor = "Dr. Carlos Rodríguez",
                Fecha = new DateTime(2027, 1, 12, 8, 30, 0),
                Descripcion = "Realización de electrocardiograma en reposo junto con evaluación del ritmo cardíaco por parte del especialista.",
                ImagenUrl = "https://picsum.photos/seed/aquaclinic3/300/200",
                EspecialidadId = cardiologiaGuid
            });

            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = Guid.Parse("{62787623-4C52-43FE-B0C9-B7044FB5929B}"),
                Nombre = "Chequeo pediátrico anual",
                Precio = 25,
                Doctor = "Dra. Ana Martínez",
                Fecha = new DateTime(2027, 1, 25, 16, 0, 0),
                Descripcion = "Revisión pediátrica anual con control de crecimiento, desarrollo y calendario vacunal del niño.",
                ImagenUrl = "https://picsum.photos/seed/aquaclinic4/300/200",
                EspecialidadId = pediatriaGuid
            });

            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = Guid.Parse("{1BABD057-E980-4CB3-9CD2-7FDD9E525668}"),
                Nombre = "Evaluación traumatológica deportiva",
                Precio = 400,
                Doctor = "Dr. Luis Fernández",
                Fecha = new DateTime(2027, 7, 8, 11, 0, 0),
                Descripcion = "Estudio completo del aparato locomotor orientado a deportistas, con pruebas de movilidad y plan de recuperación.",
                ImagenUrl = "https://picsum.photos/seed/aquaclinic5/300/200",
                EspecialidadId = traumatologiaGuid
            });

            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = Guid.Parse("{ADC42C09-08C1-4D2C-9F96-2D15BB1AF299}"),
                Nombre = "Revisión dermatológica completa",
                Precio = 135,
                Doctor = "Dra. Laura Sánchez",
                Fecha = new DateTime(2027, 5, 30, 15, 0, 0),
                Descripcion = "Mapeo corporal de lunares y lesiones cutáneas con dermatoscopia digital para la detección temprana de alteraciones.",
                ImagenUrl = "https://picsum.photos/seed/aquaclinic6/300/200",
                EspecialidadId = dermatologiaGuid
            });

            modelBuilder.Entity<Consulta>().HasData(new Consulta
            {
                Id = Guid.Parse("{7E94BC5B-71A5-4C8C-BC3B-71BB7976237E}"),
                TotalConsulta = 400,
                Pagada = true,
                FechaSolicitada = new DateTime(2026, 9, 10, 9, 0, 0),
                UserId = Guid.Parse("{A441EB40-9636-4EE6-BE49-A66C5EC1330B}")
            });

            modelBuilder.Entity<Consulta>().HasData(new Consulta
            {
                Id = Guid.Parse("{86D3A045-B42D-4854-8150-D6A374948B6E}"),
                TotalConsulta = 135,
                Pagada = true,
                FechaSolicitada = new DateTime(2026, 9, 11, 9, 0, 0),
                UserId = Guid.Parse("{AC3CFAF5-34FD-4E4D-BC04-AD1083DDC340}")
            });

            modelBuilder.Entity<Consulta>().HasData(new Consulta
            {
                Id = Guid.Parse("{771CCA4B-066C-4AC7-B3DF-4D12837FE7E0}"),
                TotalConsulta = 85,
                Pagada = true,
                FechaSolicitada = new DateTime(2026, 9, 12, 9, 0, 0),
                UserId = Guid.Parse("{D97A15FC-0D32-41C6-9DDF-62F0735C4C1C}")
            });

            modelBuilder.Entity<Consulta>().HasData(new Consulta
            {
                Id = Guid.Parse("{3DCB3EA0-80B1-4781-B5C0-4D85C41E55A6}"),
                TotalConsulta = 245,
                Pagada = true,
                FechaSolicitada = new DateTime(2026, 9, 13, 9, 0, 0),
                UserId = Guid.Parse("{4AD901BE-F447-46DD-BCF7-DBE401AFA203}")
            });

            modelBuilder.Entity<Consulta>().HasData(new Consulta
            {
                Id = Guid.Parse("{E6A2679C-79A3-4EF1-A478-6F4C91B405B6}"),
                TotalConsulta = 142,
                Pagada = true,
                FechaSolicitada = new DateTime(2026, 9, 14, 9, 0, 0),
                UserId = Guid.Parse("{7AEB2C01-FE8E-4B84-A5BA-330BDF950F5C}")
            });

            modelBuilder.Entity<Consulta>().HasData(new Consulta
            {
                Id = Guid.Parse("{F5A6A3A0-4227-4973-ABB5-A63FBE725923}"),
                TotalConsulta = 40,
                Pagada = true,
                FechaSolicitada = new DateTime(2026, 9, 15, 9, 0, 0),
                UserId = Guid.Parse("{F5A6A3A0-4227-4973-ABB5-A63FBE725923}")
            });

            modelBuilder.Entity<Consulta>().HasData(new Consulta
            {
                Id = Guid.Parse("{BA0EB0EF-B69B-46FD-B8E2-41B4178AE7CB}"),
                TotalConsulta = 116,
                Pagada = true,
                FechaSolicitada = new DateTime(2026, 9, 16, 9, 0, 0),
                UserId = Guid.Parse("{7AEB2C01-FE8E-4B84-A5BA-330BDF950F5C}")
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.CreatedBy = _loggedInUserService.UserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.Now;
                        entry.Entity.LastModifiedBy = _loggedInUserService.UserId;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
