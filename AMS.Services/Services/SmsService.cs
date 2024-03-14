using AMS.Services.Services.IServices;
using AMS.Services.Utility;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AMS.Services.Services
{
    public class SmsService : ISmsService
    {
        private readonly TwilioOptions _twilioOptions;

        public SmsService(IOptions<TwilioOptions> twilioOptions)
        {
            _twilioOptions = twilioOptions.Value;
            TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
        }

        public async Task SendMessageAsync(string to, string message)
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_twilioOptions.PhoneNumber),
                to: new PhoneNumber(to)
            );
        }
    }
}
