using AquaClinic.Application.Models.Mail;

namespace AquaClinic.Application.Contracts.Infrastructure
{
    public interface IEmailService
    {
        Task<bool> SendEmail(Email email);
    }
}
