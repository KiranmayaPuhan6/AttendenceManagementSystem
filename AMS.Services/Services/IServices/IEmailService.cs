using AMS.Services.Utility;

namespace AMS.Services.Services.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailAddress emailAddress);
    }
}
