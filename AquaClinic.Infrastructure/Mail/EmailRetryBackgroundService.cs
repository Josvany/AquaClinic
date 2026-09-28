using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Models.Mail;
using AquaClinic.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AquaClinic.Infrastructure.Mail
{
    /// <summary>
    /// Reintenta periodicamente los correos de cita que quedaron Pendientes.
    /// Backoff creciente entre intentos (1, 5, 15, 60 min); al quinto intento
    /// fallido el correo se marca Fallido (estado final auditable). Un fallo
    /// del proveedor nunca detiene el servicio.
    /// </summary>
    public class EmailRetryBackgroundService : BackgroundService
    {
        private const int MaximoIntentos = 5;
        private const int LotePorCiclo = 10;
        private static readonly TimeSpan IntervaloCiclo = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan[] Backoffs =
        {
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(60)
        };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailRetryBackgroundService> _logger;

        public EmailRetryBackgroundService(
            IServiceScopeFactory scopeFactory,
            IEmailService emailService,
            ILogger<EmailRetryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _emailService = emailService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(IntervaloCiclo);

            while (!stoppingToken.IsCancellationRequested
                && await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await ProcesarPendientesAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Apagado del host: salir limpiamente
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo de reintentos de correos: {Mensaje}", ex.Message);
                }
            }
        }

        private async Task ProcesarPendientesAsync(CancellationToken stoppingToken)
        {
            // Scope propio: el DbContext del repositorio es scoped y este servicio es singleton
            using var scope = _scopeFactory.CreateScope();
            var emailsPendientes = scope.ServiceProvider.GetRequiredService<IEmailPendienteRepository>();

            var ahoraUtc = DateTime.UtcNow;
            var pendientes = await emailsPendientes
                .GetPendientesParaReintentoAsync(LotePorCiclo, ahoraUtc)
                .ConfigureAwait(false);

            foreach (var email in pendientes)
            {
                stoppingToken.ThrowIfCancellationRequested();

                // Backoff: el intento N solo se reintenta pasados Backoffs[N-1]
                if (email.Intentos >= 1 && email.Intentos - 1 < Backoffs.Length)
                {
                    var proximo = email.UltimoIntentoUtc!.Value + Backoffs[email.Intentos - 1];
                    if (proximo > ahoraUtc)
                    {
                        continue;
                    }
                }

                try
                {
                    var enviado = await _emailService.SendEmail(new Email
                    {
                        To = email.Destinatario,
                        Subject = email.Asunto,
                        Body = email.Cuerpo
                    }).ConfigureAwait(false);

                    email.Intentos++;
                    email.UltimoIntentoUtc = DateTime.UtcNow;

                    if (enviado)
                    {
                        email.Estado = EstadoEmail.Enviado;
                        email.EnviadoUtc = DateTime.UtcNow;
                        email.UltimoError = null;
                        _logger.LogInformation(
                            "Confirmacion de la cita {CitaId} enviada en el reintento {Intento}",
                            email.CitaId, email.Intentos);
                    }
                    else if (email.Intentos >= MaximoIntentos)
                    {
                        email.Estado = EstadoEmail.Fallido;
                        email.UltimoError = "Agotados los reintentos: el proveedor rechazo el envio.";
                        _logger.LogError(
                            "Confirmacion de la cita {CitaId} marcada Fallida tras {Intentos} intentos",
                            email.CitaId, email.Intentos);
                    }
                    else
                    {
                        email.UltimoError = "El proveedor de correo rechazo el envio.";
                    }
                }
                catch (Exception ex)
                {
                    email.Intentos++;
                    email.UltimoIntentoUtc = DateTime.UtcNow;

                    if (email.Intentos >= MaximoIntentos)
                    {
                        email.Estado = EstadoEmail.Fallido;
                        email.UltimoError = $"Agotados los reintentos: {ex.Message}";
                        _logger.LogError(ex,
                            "Confirmacion de la cita {CitaId} marcada Fallida tras {Intentos} intentos",
                            email.CitaId, email.Intentos);
                    }
                    else
                    {
                        email.UltimoError = ex.Message;
                        _logger.LogWarning(ex,
                            "Reintento {Intentos} fallo para la cita {CitaId}: {Mensaje}",
                            email.Intentos, email.CitaId, ex.Message);
                    }
                }

                await emailsPendientes.UpdateAsync(email).ConfigureAwait(false);
            }
        }
    }
}
