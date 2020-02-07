using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using vykuttolib.Configuration;

namespace vykuttolib.Mail
{
    public sealed class MailgunEmailSender : IEmailSender
    {
        private readonly MailgunConfiguration _config = new MailgunConfiguration();

        public MailgunEmailSender(IConfiguration config)
        {
            config.GetSection("Mailgun").Bind(_config);
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            RestClient client = new RestClient
            {
                BaseUrl = new Uri(_config.MailgunAPI),
                Authenticator = new HttpBasicAuthenticator("api", _config.APIKey)
            };

            RestRequest request = new RestRequest();
            request.AddParameter("domain", _config.Domain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", $"{_config.SenderName} <{_config.SenderHost}>");
            request.AddParameter("to", email);
            request.AddParameter("subject", subject);
            request.AddParameter("html", htmlMessage);
            request.Method = Method.POST;

            return client.ExecuteAsync(request);
        }
    }
}
