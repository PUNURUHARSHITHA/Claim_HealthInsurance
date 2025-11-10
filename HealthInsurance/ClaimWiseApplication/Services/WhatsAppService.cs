using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using ClaimWise.Application.Interfaces;

namespace ClaimWise.Application.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public WhatsAppService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _fromNumber = configuration["Twilio:FromNumber"];

            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendMessageAsync(string toPhoneNumber, string message)


        {
            try
            {
                var to = new PhoneNumber($"whatsapp:{toPhoneNumber}");
                var from = new PhoneNumber(_fromNumber);

                await MessageResource.CreateAsync(
                    to: to,
                    from: from,
                    body: message
                );

                // Optional: log success
                // Console.WriteLine($"WhatsApp message sent to {toPhoneNumber}");
            }
            catch (Exception ex)
            {
                // Optional: log error
                // Console.WriteLine($"Failed to send WhatsApp message: {ex.Message}");
                throw new ApplicationException("Failed to send WhatsApp message", ex);
            }
        }
    }
}
