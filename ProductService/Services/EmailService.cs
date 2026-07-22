using Microsoft.Extensions.Options;
using ProductService.Models;
using System.Net;
using System.Net.Mail;

namespace ProductService.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var smtp = new SmtpClient(_settings.Host, _settings.Port);

            // SMTP Authentication
            smtp.EnableSsl = _settings.EnableSSL;

            smtp.Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password);

            var message = new MailMessage
            {
                From = new MailAddress(
                    _settings.SenderEmail,
                    _settings.SenderName),

                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await smtp.SendMailAsync(message);
        }
    }
}