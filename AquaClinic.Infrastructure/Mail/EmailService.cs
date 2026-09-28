using AquaClinic.Application.Contracts.Infrastructure;
using AquaClinic.Application.Models.Mail;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AquaClinic.Infrastructure.Mail
{
    /// <summary>
    /// Envio de correos transaccionales via Mailjet (SDK oficial Mailjet.Api).
    /// Autenticacion Basic con API key publica + secreta; el remitente debe
    /// estar verificado en la cuenta de Mailjet de la clinica.
    /// </summary>
    public class EmailService : IEmailService
    {
        public EmailSettings _emailSettings { get; }
        public ILogger<EmailService> _logger { get; }

        public EmailService(IOptions<EmailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmail(Email email)
        {
            try
            {
                var client = new MailjetClient(_emailSettings.ApiKey, _emailSettings.SecretKey);

                var mensaje = new TransactionalEmailBuilder()
                    .WithFrom(new SendContact(_emailSettings.FromAddress, _emailSettings.FromName))
                    .WithTo(new SendContact(email.To))
                    .WithSubject(email.Subject)
                    .WithHtmlPart(email.Body)
                    .WithTextPart(email.Body)
                    .Build();

                var response = await client.SendTransactionalEmailAsync(mensaje);

                // Mailjet responde un mensaje por peticion con Status "success" o
                // "error"; los errores del procesamiento vienen en Errors[].
                var primerMensaje = response.Messages is not null && response.Messages.Length > 0
                    ? response.Messages[0]
                    : null;

                if (primerMensaje is null)
                {
                    _logger.LogError(
                        "Email sending failed a {Destinatario}: Mailjet no devolvio mensajes",
                        email.To);
                    return false;
                }

                if (!string.Equals(primerMensaje.Status, "success", StringComparison.OrdinalIgnoreCase))
                {
                    var detalle = ResumirErrores(primerMensaje.Errors);
                    _logger.LogError(
                        "Email sending failed a {Destinatario}: status {Status}{Detalle}",
                        email.To, primerMensaje.Status, detalle);
                    return false;
                }

                _logger.LogInformation("Email sent a {Destinatario} via Mailjet", email.To);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sending failed a {Destinatario}: {Mensaje}", email.To, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Une los errores que Mailjet reporta por el mensaje (codigo, status HTTP,
        /// descripcion y campo del payload relacionado) en una sola linea para el log.
        /// </summary>
        private static string ResumirErrores(List<SendEmailError>? errores)
        {
            if (errores is null || errores.Count == 0)
            {
                return string.Empty;
            }

            var partes = errores.Select(e =>
            {
                var relacionado = e.ErrorRelatedTo is { Length: > 0 }
                    ? $" (relacionado con: {string.Join(", ", e.ErrorRelatedTo)})"
                    : string.Empty;
                return $"[{e.ErrorCode}/{e.StatusCode}] {e.ErrorMessage}{relacionado}";
            });

            return ": " + string.Join("; ", partes);
        }
    }
}
