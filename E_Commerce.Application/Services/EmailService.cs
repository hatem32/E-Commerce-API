using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace E_Commerce.Application.Services
{
    // Uses .NET's built-in SmtpClient - no extra package needed. Works with Gmail
    // (via an "app password"), Outlook, or any standard SMTP provider (SendGrid,
    // Mailgun, etc. all offer plain SMTP credentials too).
    internal class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, string? replyToEmail = null, CancellationToken ct = default)
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            if (!string.IsNullOrWhiteSpace(replyToEmail))
            {
                message.ReplyToList.Add(new MailAddress(replyToEmail));
            }

            await client.SendMailAsync(message, ct);
        }
    }
}