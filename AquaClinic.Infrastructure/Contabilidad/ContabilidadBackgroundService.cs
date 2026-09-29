using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Citas;
using AquaClinic.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Infrastructure.Contabilidad
{
    /// <summary>
    /// Worker de mini-contabilidad: cada 15 minutos registra en MovimientosServicios
    /// las citas ya atendidas (Confirmada o Completada cuya FechaFin paso) y recalcula
    /// el ResumenDiarioServicio de los dias afectados. El criterio de elegibilidad vive
    /// en IMovimientoServicioRepository (unico punto de verdad); el indice unico
    /// (CitaId, ServicioId) garantiza que una cita nunca se cuente dos veces.
    /// Un fallo del ciclo nunca detiene el servicio.
    /// </summary>
    public class ContabilidadBackgroundService : BackgroundService
    {
        private static readonly TimeSpan IntervaloCiclo = TimeSpan.FromMinutes(15);
        private const int MaximoCitasPorCiclo = 500;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ContabilidadBackgroundService> _logger;

        public ContabilidadBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ContabilidadBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker de contabilidad iniciado (intervalo: {Intervalo})", IntervaloCiclo);

            using var timer = new PeriodicTimer(IntervaloCiclo);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarCicloAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Apagado del host: salir limpiamente
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo de contabilidad: {Mensaje}", ex.Message);
                }

                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Worker de contabilidad detenido");
        }

        private async Task ProcesarCicloAsync(CancellationToken stoppingToken)
        {
            // Scope propio: el DbContext del repositorio es scoped y este servicio es singleton
            using var scope = _scopeFactory.CreateScope();
            var movimientosRepo = scope.ServiceProvider.GetRequiredService<IMovimientoServicioRepository>();

            var ahoraUtc = DateTimeOffset.UtcNow;

            var citas = await movimientosRepo
                .GetCitasParaRegistrarAsync(ahoraUtc)
                .ConfigureAwait(false);

            if (citas.Count == 0)
            {
                return;
            }

            // Proteccion contra atrasos enormes (primer arranque con historial largo)
            if (citas.Count > MaximoCitasPorCiclo)
            {
                citas = citas
                    .OrderBy(c => c.FechaFin)
                    .Take(MaximoCitasPorCiclo)
                    .ToList();
            }

            var nuevosMovimientos = new List<MovimientoServicio>();
            var fechasLocalesAfectadas = new HashSet<DateTime>();

            foreach (var cita in citas)
            {
                stoppingToken.ThrowIfCancellationRequested();

                foreach (var cs in cita.CitasServicios ?? Enumerable.Empty<CitaServicio>())
                {
                    nuevosMovimientos.Add(new MovimientoServicio
                    {
                        CitaId = cita.CitaId,
                        ServicioId = cs.ServicioId,
                        PersonalId = cita.PersonalId,
                        UsuarioId = cita.UsuarioId,
                        // Snapshot del cliente: invitados por nombre; registrados solo por UsuarioId
                        NombreCliente = cita.UsuarioId == null ? cita.NombreInvitado : null,
                        PrecioSnapshot = cs.Servicio.Precio,
                        FechaAtencion = cita.FechaFin
                    });

                    fechasLocalesAfectadas.Add(CitaFechas.ALocal(cita.FechaFin).Date);
                }
            }

            if (nuevosMovimientos.Count == 0)
            {
                return;
            }

            await movimientosRepo.AgregarMovimientosAsync(nuevosMovimientos).ConfigureAwait(false);

            var movimientosRecalculados = await RecalcularResumenesAsync(
                movimientosRepo, fechasLocalesAfectadas, stoppingToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Contabilidad: {Citas} citas -> {Movimientos} movimientos registrados; {Grupos} resumenes diarios recalculados",
                citas.Count, nuevosMovimientos.Count, movimientosRecalculados);
        }

        /// <summary>
        /// Recalcula (no incrementa) el resumen de cada dia x servicio con movimientos
        /// en las fechas locales afectadas: si un ciclo anterior fallo a medias, el
        /// siguiente deja los totales consistentes.
        /// </summary>
        private static async Task<int> RecalcularResumenesAsync(
            IMovimientoServicioRepository movimientosRepo,
            HashSet<DateTime> fechasLocales,
            CancellationToken stoppingToken)
        {
            var gruposRecalculados = 0;

            foreach (var fechaLocal in fechasLocales)
            {
                stoppingToken.ThrowIfCancellationRequested();

                // Dia contable local -> ventana UTC [inicio, siguiente dia)
                var offset = CitaFechas.OffsetLocal(fechaLocal);
                var desdeUtc = new DateTimeOffset(fechaLocal, offset);
                var hastaUtc = desdeUtc.AddDays(1);

                var movimientosDelDia = await movimientosRepo
                    .GetMovimientosEntreAsync(desdeUtc, hastaUtc)
                    .ConfigureAwait(false);

                foreach (var grupo in movimientosDelDia.GroupBy(m => m.ServicioId))
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var existente = await movimientosRepo
                        .GetResumenAsync(fechaLocal, grupo.Key)
                        .ConfigureAwait(false);

                    var resumen = existente ?? new ResumenDiarioServicio
                    {
                        FechaLocal = fechaLocal,
                        ServicioId = grupo.Key
                    };

                    resumen.CantidadAtenciones = grupo.Count();
                    resumen.MontoTotal = grupo.Sum(m => m.PrecioSnapshot);
                    resumen.ActualizadoUtc = DateTime.UtcNow;

                    await movimientosRepo.GuardarResumenAsync(resumen).ConfigureAwait(false);
                    gruposRecalculados++;
                }
            }

            return gruposRecalculados;
        }
    }
}
