using AMS.Services.Services.IServices;
using AMS.Services.Utility;
using AMS.Services.Utility.HelperMethods;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AMS.Services.Services
{
    public class SmsService : ISmsService
    {
        private readonly TwilioOptions _twilioOptions;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IOptions<TwilioOptions> twilioOptions,ILogger<SmsService> logger)
        {
            _twilioOptions = twilioOptions.Value;
            TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
            _logger = logger;
        }

        public async Task SendMessageAsync(string to, string message)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_twilioOptions.PhoneNumber),
                to: new PhoneNumber(to)
            );
            _logger.LogDebug($"Message sent successfully");
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended.");
        }
    }
}
