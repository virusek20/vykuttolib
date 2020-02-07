using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace vykuttolib.Mail
{
    public class LoggerEmailSender : IEmailSender
    {
        private readonly ILogger<LoggerEmailSender> _logger;

        public LoggerEmailSender(ILogger<LoggerEmailSender> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation($"Sending email to: '{email}' - '{subject}'\n{htmlMessage}");
        }
    }
}
