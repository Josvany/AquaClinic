using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using AquaClinic.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace AquaClinic.Persistence.IntegrationTests
{
    /// <summary>
    /// Verifica el criterio de contabilidad: solo citas aceptadas (Confirmada o
    /// Completada) cuya FechaFin ya paso, nunca Pendiente ni Cancelada, y que una
    /// cita ya procesada no se cuente dos veces (idempotencia del worker).
    /// </summary>
    public class MovimientoServicioRepositoryTests
    {
        private readonly AquaClinicDbContext _dbContext;
        private readonly MovimientoServicioRepository _repository;

        public MovimientoServicioRepositoryTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AquaClinicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var loggedInUserServiceMock = new Mock<Application.Contracts.ILoggedInUserService>();
            loggedInUserServiceMock.Setup(m => m.UserId).Returns("usuario-de-pruebas");

            _dbContext = new AquaClinicDbContext(dbContextOptions, loggedInUserServiceMock.Object);
            _repository = new MovimientoServicioRepository(_dbContext);
        }

        private async Task<(Especialidad Especialidad, Servicio Servicio)> SembrarServicioAsync(
            string nombre, decimal precio)
        {
            var especialidad = new Especialidad { Nombre = $"Esp {nombre}" };
            _dbContext.Especialidades.Add(especialidad);

            var servicio = new Servicio
            {
                Nombre = nombre,
                Precio = precio,
                DuracionMinutos = 60,
                Activo = true,
                EspecialidadId = especialidad.EspecialidadId
            };
            _dbContext.Servicios.Add(servicio);

            await _dbContext.SaveChangesAsync();
            return (especialidad, servicio);
        }

        private static Cita CrearCita(EstadoCita estado, DateTimeOffset fechaInicio, DateTimeOffset fechaFin)
        {
            return new Cita
            {
                NombreInvitado = "Ana Torres",
                EmailInvitado = "ana.torres@example.com",
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Estado = estado
            };
        }

        [Fact]
        public async Task GetCitasParaRegistrarAsync_SoloDevuelveAceptadasConFechaPasada()
        {
            var (_, servicio) = await SembrarServicioAsync("Limpieza profunda", 120m);
            var ahora = DateTimeOffset.UtcNow;

            var confirmadaPasada = CrearCita(EstadoCita.Confirmada, ahora.AddHours(-3), ahora.AddHours(-2));
            var completadaPasada = CrearCita(EstadoCita.Completada, ahora.AddHours(-5), ahora.AddHours(-4));
            var pendientePasada = CrearCita(EstadoCita.Pendiente, ahora.AddHours(-7), ahora.AddHours(-6));
            var canceladaPasada = CrearCita(EstadoCita.Cancelada, ahora.AddHours(-9), ahora.AddHours(-8));
            var confirmadaFutura = CrearCita(EstadoCita.Confirmada, ahora.AddHours(1), ahora.AddHours(2));

            _dbContext.Citas.AddRange(confirmadaPasada, completadaPasada, pendientePasada, canceladaPasada, confirmadaFutura);
            await _dbContext.SaveChangesAsync();

            _dbContext.CitasServicios.AddRange(
                new CitaServicio { CitaId = confirmadaPasada.CitaId, ServicioId = servicio.ServicioId },
                new CitaServicio { CitaId = completadaPasada.CitaId, ServicioId = servicio.ServicioId },
                new CitaServicio { CitaId = pendientePasada.CitaId, ServicioId = servicio.ServicioId },
                new CitaServicio { CitaId = canceladaPasada.CitaId, ServicioId = servicio.ServicioId },
                new CitaServicio { CitaId = confirmadaFutura.CitaId, ServicioId = servicio.ServicioId });
            await _dbContext.SaveChangesAsync();

            var elegibles = await _repository.GetCitasParaRegistrarAsync(ahora.AddMinutes(1));

            var ids = elegibles.Select(c => c.CitaId).ToList();
            ids.Count.ShouldBe(2);
            ids.ShouldContain(confirmadaPasada.CitaId);   // la de las 10 AM confirmada: cuenta
            ids.ShouldContain(completadaPasada.CitaId);
            ids.ShouldNotContain(pendientePasada.CitaId); // pendiente: nunca cuenta
            ids.ShouldNotContain(canceladaPasada.CitaId); // cancelada: nunca cuenta
            ids.ShouldNotContain(confirmadaFutura.CitaId); // aun no se atiende
        }

        [Fact]
        public async Task GetCitasParaRegistrarAsync_ExcluyeCitasYaProcesadas()
        {
            var (_, servicio) = await SembrarServicioAsync("Microneedling", 250m);
            var ahora = DateTimeOffset.UtcNow;

            var cita = CrearCita(EstadoCita.Confirmada, ahora.AddHours(-2), ahora.AddHours(-1));
            _dbContext.Citas.Add(cita);
            _dbContext.CitasServicios.Add(new CitaServicio
            {
                CitaId = cita.CitaId,
                ServicioId = servicio.ServicioId
            });
            await _dbContext.SaveChangesAsync();

            // Primera pasada: la cita es elegible
            var primera = await _repository.GetCitasParaRegistrarAsync(ahora.AddMinutes(1));
            primera.Single().CitaId.ShouldBe(cita.CitaId);

            // El worker registra el movimiento
            await _repository.AgregarMovimientosAsync(new[]
            {
                new MovimientoServicio
                {
                    CitaId = cita.CitaId,
                    ServicioId = servicio.ServicioId,
                    PrecioSnapshot = servicio.Precio,
                    FechaAtencion = cita.FechaFin
                }
            });

            // Segunda pasada: la cita ya no vuelve a contarse
            var segunda = await _repository.GetCitasParaRegistrarAsync(ahora.AddMinutes(1));
            segunda.ShouldBeEmpty();
        }

        [Fact]
        public async Task GuardarResumenAsync_InsertaYActualizaUnaSolaFila()
        {
            var (_, servicio) = await SembrarServicioAsync("Botox", 350m);
            var fechaLocal = new DateTime(2026, 9, 29);

            var resumen = new ResumenDiarioServicio
            {
                FechaLocal = fechaLocal,
                ServicioId = servicio.ServicioId,
                CantidadAtenciones = 1,
                MontoTotal = 350m,
                ActualizadoUtc = DateTime.UtcNow
            };
            await _repository.GuardarResumenAsync(resumen);

            resumen.CantidadAtenciones = 3;
            resumen.MontoTotal = 1050m;
            await _repository.GuardarResumenAsync(resumen);

            var filas = await _dbContext.ResumenesDiarios
                .Where(r => r.FechaLocal == fechaLocal && r.ServicioId == servicio.ServicioId)
                .ToListAsync();

            filas.Count.ShouldBe(1);
            filas.Single().CantidadAtenciones.ShouldBe(3);
            filas.Single().MontoTotal.ShouldBe(1050m);
        }

        [Fact]
        public async Task GetMovimientosEntreAsync_DevuelveSoloLaVentanaUtc()
        {
            var (_, servicio) = await SembrarServicioAsync("Radiofrecuencia", 140m);
            var fechaLocal = new DateTime(2026, 9, 29);
            var offset = TimeSpan.FromHours(-7);

            var dentro = new MovimientoServicio
            {
                ServicioId = servicio.ServicioId,
                PrecioSnapshot = 140m,
                FechaAtencion = new DateTimeOffset(2026, 9, 29, 10, 0, 0, offset)
            };
            var fuera = new MovimientoServicio
            {
                ServicioId = servicio.ServicioId,
                PrecioSnapshot = 140m,
                FechaAtencion = new DateTimeOffset(2026, 9, 28, 10, 0, 0, offset)
            };
            await _repository.AgregarMovimientosAsync(new[] { dentro, fuera });

            var desdeUtc = new DateTimeOffset(fechaLocal, offset);
            var movimientos = await _repository.GetMovimientosEntreAsync(desdeUtc, desdeUtc.AddDays(1));

            movimientos.Count.ShouldBe(1);
            movimientos.Single().MovimientoServicioId.ShouldBe(dentro.MovimientoServicioId);
        }
    }
}
