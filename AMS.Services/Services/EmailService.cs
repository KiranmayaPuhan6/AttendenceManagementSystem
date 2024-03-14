using AMS.Services.Services.IServices;
using AMS.Services.Utility;
using AMS.Services.Utility.HelperMethods;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace AMS.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> emailOptions,ILogger<EmailService> logger)
        {
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailAddress emailAddress)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailOptions.Username));
            email.To.Add(MailboxAddress.Parse(emailAddress.To));
            email.Subject = emailAddress.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = emailAddress.Message };
            using var smtp = new SmtpClient();
            smtp.Connect(_emailOptions.Host, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_emailOptions.Username, _emailOptions.Password);
            smtp.Send(email);
            smtp.Disconnect(true);
            _logger.LogDebug($"Mail sent successfully");
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
        }
    }
}
