using AquaClinic.Application.Contracts;
using AquaClinic.Domain.Common;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AquaClinic.Persistence
{
    /// <summary>
    /// Contexto &#250;nico de la aplicacion: incluye el esquema de ASP.NET Identity
    /// (AspNetUsers, AspNetRoles, ...) y las tablas del negocio de la clinica.
    /// Los Ids son UUID v7 generados en los constructores de las entidades.
    /// Las fechas de citas y bloqueos se almacenan en UTC (datetimeoffset);
    /// la hora local de la clinica es America/Vancouver (PST/PDT).
    /// </summary>
    public class AquaClinicDbContext : IdentityDbContext<ApplicationUser>
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

        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<CitaServicio> CitasServicios { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<PersonalEspecialidad> PersonalEspecialidades { get; set; }
        public DbSet<HorarioClinica> HorariosClinica { get; set; }
        public DbSet<Bloqueo> Bloqueos { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<PromocionServicio> PromocionesServicios { get; set; }
        public DbSet<Cupon> Cupones { get; set; }
        public DbSet<MovimientoPuntos> MovimientosPuntos { get; set; }
        public DbSet<EmailPendiente> EmailsPendientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Esquema de Identity (AspNet*) para la base de datos &#250;nica
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AquaClinicDbContext).Assembly);

            // Seed con valores ESTATICOS (regla: nunca DateTime.Now ni Guid.NewGuid en HasData).
            // Las fechas usan offsets reales de America/Vancouver (PDT -07:00 / PST -08:00).
            var facialesGuid = Guid.Parse("01a0bc01-2484-76f6-add2-b68394c7f1ac");
            var avanzadoGuid = Guid.Parse("01a0bc01-24e2-7db8-a95a-7f4a793a97f6");
            var inyectablesGuid = Guid.Parse("01a0bc01-24f4-79e6-a0f3-e83a946f9209");
            var cejasGuid = Guid.Parse("01a0bc01-2505-7a4e-a157-2f9393424890");

            var fechaAuditoria = new DateTime(2026, 8, 1);

            // ===== Especialidades =====
            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = facialesGuid,
                Nombre = "Tratamientos Faciales",
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = avanzadoGuid,
                Nombre = "Cuidado Avanzado de la Piel",
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = inyectablesGuid,
                Nombre = "Tratamientos Inyectables",
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Especialidad>().HasData(new Especialidad
            {
                EspecialidadId = cejasGuid,
                Nombre = "Cejas",
                CreatedDate = fechaAuditoria
            });

            // ===== Servicios =====
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2514-7922-a163-24d411678c6f"),
                Nombre = "Limpieza profunda personalizada",
                Descripcion = "Limpieza facial profunda adaptada a tu tipo de piel, con extracciones e hidratación final.",
                Precio = 120.00m,
                DuracionMinutos = 60,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic1/300/200",
                Activo = true,
                EspecialidadId = facialesGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2523-78d0-a114-6d55dac0a477"),
                Nombre = "Hidrodermoabrasión",
                Descripcion = "Exfoliación e hidratación con tecnología de vortex-fusion para renovar y revitalizar la piel.",
                Precio = 110.00m,
                DuracionMinutos = 45,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic2/300/200",
                Activo = true,
                EspecialidadId = facialesGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2533-7e91-af1e-b75928c66155"),
                Nombre = "Facial de alta frecuencia",
                Descripcion = "Tratamiento con alta frecuencia diseñado para hidratar y renovar la piel del rostro.",
                Precio = 95.00m,
                DuracionMinutos = 45,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic3/300/200",
                Activo = true,
                EspecialidadId = facialesGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2543-7a75-ad35-6cbe60d25a62"),
                Nombre = "Microneedling",
                Descripcion = "Microinfundido para mejorar la textura de la piel y reducir las cicatrices del acné.",
                Precio = 250.00m,
                DuracionMinutos = 60,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic4/300/200",
                Activo = true,
                EspecialidadId = avanzadoGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2553-77a8-a7b9-9d9b607248a5"),
                Nombre = "Radiofrecuencia",
                Descripcion = "Tratamiento de radiofrecuencia para estimular la producción natural de colágeno.",
                Precio = 140.00m,
                DuracionMinutos = 30,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic5/300/200",
                Activo = true,
                EspecialidadId = avanzadoGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2563-7d94-abbc-b9a1df6195cc"),
                Nombre = "Botox",
                Descripcion = "Toxina botulínica para combatir las líneas de expresión y el envejecimiento facial.",
                Precio = 350.00m,
                DuracionMinutos = 30,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic6/300/200",
                Activo = true,
                EspecialidadId = inyectablesGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2573-7b42-a339-14023acbc2eb"),
                Nombre = "Bioestimuladores",
                Descripcion = "Bioestimuladores de colágeno para un rejuvenecimiento facial progresivo y natural.",
                Precio = 400.00m,
                DuracionMinutos = 45,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic7/300/200",
                Activo = true,
                EspecialidadId = inyectablesGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2583-7f61-a561-01f4cf10e04a"),
                Nombre = "Laminado de cejas",
                Descripcion = "Laminado para lograr cejas más densas, fijas y con una forma definida.",
                Precio = 85.00m,
                DuracionMinutos = 60,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic8/300/200",
                Activo = true,
                EspecialidadId = cejasGuid,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Servicio>().HasData(new Servicio
            {
                ServicioId = Guid.Parse("01a0bc01-2593-79db-aec9-9e1d37b12b55"),
                Nombre = "Diseño y perfilado de cejas",
                Descripcion = "Diseño, perfilado y tinte de cejas para realzar la mirada.",
                Precio = 65.00m,
                DuracionMinutos = 45,
                ImagenUrl = "https://picsum.photos/seed/aquaclinic9/300/200",
                Activo = true,
                EspecialidadId = cejasGuid,
                CreatedDate = fechaAuditoria
            });

            // ===== Personal =====
            var walnerGuid = Guid.Parse("01a0bc01-25a3-7f89-a190-15ac098e2d00");
            var mariaGuid = Guid.Parse("01a0bc01-25b2-7f49-a1e7-e4e7afb2645e");
            var carolinaGuid = Guid.Parse("01a0bc01-25c2-7148-a710-befdeeb20ef7");

            modelBuilder.Entity<Personal>().HasData(new Personal
            {
                PersonalId = walnerGuid,
                Nombre = "Walner",
                Apellidos = "Solano",
                Telefono = "+1 604-555-0001",
                Email = "walner@aquaskinandglow.ca",
                Activo = true,
                ApplicationUserId = null,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Personal>().HasData(new Personal
            {
                PersonalId = mariaGuid,
                Nombre = "María Fernanda",
                Apellidos = "Rojas",
                Telefono = "+1 604-555-0002",
                Email = "maria.rojas@aquaskinandglow.ca",
                Activo = true,
                ApplicationUserId = null,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Personal>().HasData(new Personal
            {
                PersonalId = carolinaGuid,
                Nombre = "Carolina",
                Apellidos = "Méndez",
                Telefono = "+1 604-555-0003",
                Email = "carolina.mendez@aquaskinandglow.ca",
                Activo = true,
                ApplicationUserId = null,
                CreatedDate = fechaAuditoria
            });

            // ===== Personal por especialidad =====
            modelBuilder.Entity<PersonalEspecialidad>().HasData(
                new { PersonalId = walnerGuid, EspecialidadId = facialesGuid },
                new { PersonalId = walnerGuid, EspecialidadId = inyectablesGuid },
                new { PersonalId = mariaGuid, EspecialidadId = avanzadoGuid },
                new { PersonalId = mariaGuid, EspecialidadId = facialesGuid },
                new { PersonalId = carolinaGuid, EspecialidadId = cejasGuid },
                new { PersonalId = carolinaGuid, EspecialidadId = facialesGuid });

            // ===== Horarios de la clínica (hora local: Langley, BC) =====
            modelBuilder.Entity<HorarioClinica>().HasData(new HorarioClinica
            {
                HorarioId = Guid.Parse("01a0bc01-25d3-7a78-aef1-5d50aeb3e1fb"),
                DiaSemana = DayOfWeek.Sunday,
                HoraApertura = new TimeOnly(9, 0),
                HoraCierre = new TimeOnly(18, 0),
                Cerrado = true,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<HorarioClinica>().HasData(new HorarioClinica
            {
                HorarioId = Guid.Parse("01a0bc01-25f3-7b30-a329-25589a5711b3"),
                DiaSemana = DayOfWeek.Monday,
                HoraApertura = new TimeOnly(9, 0),
                HoraCierre = new TimeOnly(18, 0),
                Cerrado = false,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<HorarioClinica>().HasData(new HorarioClinica
            {
                HorarioId = Guid.Parse("01a0bc01-2602-73ac-a02e-32fbcb3bdd8f"),
                DiaSemana = DayOfWeek.Tuesday,
                HoraApertura = new TimeOnly(9, 0),
                HoraCierre = new TimeOnly(18, 0),
                Cerrado = false,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<HorarioClinica>().HasData(new HorarioClinica
            {
                HorarioId = Guid.Parse("01a0bc01-2612-7e08-a00d-f93c067a4c09"),
                DiaSemana = DayOfWeek.Wednesday,
                HoraApertura = new TimeOnly(9, 0),
                HoraCierre = new TimeOnly(18, 0),
                Cerrado = false,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<HorarioClinica>().HasData(new HorarioClinica
            {
                HorarioId = Guid.Parse("01a0bc01-2622-75cf-af4d-d77cf2210516"),
                DiaSemana = DayOfWeek.Thursday,
                HoraApertura = new TimeOnly(9, 0),
                HoraCierre = new TimeOnly(18, 0),
                Cerrado = false,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<HorarioClinica>().HasData(new HorarioClinica
            {
                HorarioId = Guid.Parse("01a0bc01-263c-7dd1-ace4-4a9fe536f4f6"),
                DiaSemana = DayOfWeek.Friday,
                HoraApertura = new TimeOnly(9, 0),
                HoraCierre = new TimeOnly(18, 0),
                Cerrado = false,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<HorarioClinica>().HasData(new HorarioClinica
            {
                HorarioId = Guid.Parse("01a0bc01-264b-77ba-a44b-31d1732df51f"),
                DiaSemana = DayOfWeek.Saturday,
                HoraApertura = new TimeOnly(10, 0),
                HoraCierre = new TimeOnly(16, 0),
                Cerrado = false,
                CreatedDate = fechaAuditoria
            });

            // ===== Bloqueos (fechas UTC con offset real: PDT -07:00 hasta nov-2026, luego PST -08:00) =====
            modelBuilder.Entity<Bloqueo>().HasData(new Bloqueo
            {
                BloqueoId = Guid.Parse("01a0bc01-265b-7685-a5ac-92c0c18cacdc"),
                Motivo = "Capacitación del equipo",
                FechaInicio = new DateTimeOffset(2026, 10, 5, 9, 0, 0, TimeSpan.FromHours(-7)),
                FechaFin = new DateTimeOffset(2026, 10, 5, 17, 0, 0, TimeSpan.FromHours(-7)),
                PersonalId = null,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Bloqueo>().HasData(new Bloqueo
            {
                BloqueoId = Guid.Parse("01a0bc01-266b-7287-a46b-2350ca53d7b4"),
                Motivo = "Vacaciones",
                FechaInicio = new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.FromHours(-8)),
                FechaFin = new DateTimeOffset(2026, 11, 6, 18, 0, 0, TimeSpan.FromHours(-8)),
                PersonalId = carolinaGuid,
                CreatedDate = fechaAuditoria
            });

            // ===== Citas de ejemplo (clientes invitados: UsuarioId = null) =====
            var cita1Guid = Guid.Parse("01a0bc01-267b-72d7-abd9-df8492b67a3e");
            var cita2Guid = Guid.Parse("01a0bc01-268a-7884-af3f-6d7b6df74218");
            var cita3Guid = Guid.Parse("01a0bc01-269a-77f7-a5f2-fbcbb5422872");

            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = cita1Guid,
                UsuarioId = null,
                NombreInvitado = "Ana Torres",
                EmailInvitado = "ana.torres@example.com",
                TelefonoInvitado = "+1 604-555-0101",
                PersonalId = mariaGuid,
                FechaInicio = new DateTimeOffset(2026, 9, 25, 15, 0, 0, TimeSpan.FromHours(-7)),
                FechaFin = new DateTimeOffset(2026, 9, 25, 16, 0, 0, TimeSpan.FromHours(-7)),
                Estado = EstadoCita.Confirmada,
                Notas = "Primera visita; piel sensible.",
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = cita2Guid,
                UsuarioId = null,
                NombreInvitado = "Luis Gómez",
                EmailInvitado = "luis.gomez@example.com",
                TelefonoInvitado = "+1 604-555-0202",
                PersonalId = mariaGuid,
                FechaInicio = new DateTimeOffset(2026, 9, 26, 11, 0, 0, TimeSpan.FromHours(-7)),
                FechaFin = new DateTimeOffset(2026, 9, 26, 12, 0, 0, TimeSpan.FromHours(-7)),
                Estado = EstadoCita.Pendiente,
                Notas = null,
                CreatedDate = fechaAuditoria
            });
            modelBuilder.Entity<Cita>().HasData(new Cita
            {
                CitaId = cita3Guid,
                UsuarioId = null,
                NombreInvitado = "Priya Sharma",
                EmailInvitado = "priya.sharma@example.com",
                TelefonoInvitado = "+1 604-555-0303",
                PersonalId = carolinaGuid,
                FechaInicio = new DateTimeOffset(2026, 10, 2, 14, 30, 0, TimeSpan.FromHours(-7)),
                FechaFin = new DateTimeOffset(2026, 10, 2, 15, 30, 0, TimeSpan.FromHours(-7)),
                Estado = EstadoCita.Pendiente,
                Notas = null,
                CreatedDate = fechaAuditoria
            });

            // Un servicio por cita (el esquema permite N opcionales)
            modelBuilder.Entity<CitaServicio>().HasData(
                new { CitaId = cita1Guid, ServicioId = Guid.Parse("01a0bc01-2514-7922-a163-24d411678c6f") },
                new { CitaId = cita2Guid, ServicioId = Guid.Parse("01a0bc01-2543-7a75-ad35-6cbe60d25a62") },
                new { CitaId = cita3Guid, ServicioId = Guid.Parse("01a0bc01-2583-7f61-a561-01f4cf10e04a") });

            // ===== Promociones (esquema de fidelidad) =====
            modelBuilder.Entity<Promocion>().HasData(new Promocion
            {
                PromocionId = Guid.Parse("01a0bc01-26a9-7fb3-a2e7-2dc235635667"),
                Nombre = "Gran inauguración",
                Descripcion = "15% de descuento en todos los servicios por la gran inauguración de la clínica.",
                TipoDescuento = TipoDescuento.Porcentaje,
                Valor = 15.00m,
                VigenciaDesde = new DateTimeOffset(2026, 8, 14, 0, 0, 0, TimeSpan.FromHours(-7)),
                VigenciaHasta = new DateTimeOffset(2026, 10, 14, 23, 59, 0, TimeSpan.FromHours(-7)),
                Activo = true,
                CreatedDate = fechaAuditoria
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
                        entry.Entity.CreatedBy = _loggedInUserService?.UserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.Now;
                        entry.Entity.LastModifiedBy = _loggedInUserService?.UserId;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
