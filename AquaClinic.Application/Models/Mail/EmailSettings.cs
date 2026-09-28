namespace AquaClinic.Application.Models.Mail
{
    /// <summary>
    /// Credenciales y remitente para Mailjet (Email API).
    /// ApiKey = clave publica, SecretKey = clave privada (Basic Auth).
    /// </summary>
    public class EmailSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}