using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using vykuttolib.Configuration;

namespace vykuttolib.Mail
{
	public sealed class SmtpEmailSender : IEmailSender, IDisposable
	{
		private readonly SmtpConfiguration _config = new SmtpConfiguration();
		private readonly SmtpClient _client;

		public SmtpEmailSender(IConfiguration config)
		{
			config.GetSection("Smtp").Bind(_config);

			_client = new SmtpClient
			{
				ServerCertificateValidationCallback = (s, c, h, e) => true
			};

		}

		public void Dispose()
		{
			_client.Dispose();
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			try
			{
				await _client.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.Auto);
				await _client.AuthenticateAsync(_config.Username, _config.Password);

				BodyBuilder bodyBuilder = new BodyBuilder
				{
					HtmlBody = htmlMessage
				};

				MimeMessage message = new MimeMessage();
				message.From.Add(new MailboxAddress(_config.SenderName, _config.SenderHost));
				message.To.Add(new MailboxAddress("", email));
				message.Subject = subject;
				message.Body = bodyBuilder.ToMessageBody();
				await _client.SendAsync(message);
				await _client.DisconnectAsync(true);
			}
			catch
			{
				Debug.WriteLine("Connection to SMTP server failed");
			}
		}
	}
}
