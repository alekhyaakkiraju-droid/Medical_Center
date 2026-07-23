using AngularApi.Options;
using AngularApi.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AngularApi.Services.impelementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IConfiguration configuration, IOptions<SmtpSettings> smtpSettings)
        {
            _configuration = configuration;
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(Message message)
        {
            var emailUsername = _configuration["EmailSettings:EmailUsername"]
                ?? throw new InvalidOperationException("EmailSettings:EmailUsername is not configured.");
            var emailPassword = _configuration["EmailSettings:EmailPassword"]
                ?? throw new InvalidOperationException("EmailSettings:EmailPassword is not configured.");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Medical Center", emailUsername));
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart("html") { Text = message.Body };

            foreach (var recipient in message.To)
            {
                emailMessage.To.Add(MailboxAddress.Parse(recipient));
            }

            using var smtpClient = new SmtpClient();
            var secureSocketOptions = _smtpSettings.UseTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await smtpClient.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, secureSocketOptions);
            await smtpClient.AuthenticateAsync(emailUsername, emailPassword);
            await smtpClient.SendAsync(emailMessage);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
