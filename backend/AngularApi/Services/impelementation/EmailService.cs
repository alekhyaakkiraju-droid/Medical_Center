using AngularApi.Contracts.Services;
using AngularApi.Options;
using AngularApi.Contracts.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Retry;

namespace AngularApi.Services.impelementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly SmtpSettings _smtpSettings;
        private readonly BaaFeatureFlags _baaFeatureFlags;
        private readonly ILogger<EmailService> _logger;
        private readonly ResiliencePipeline _retryPipeline;

        public EmailService(
            IConfiguration configuration,
            IOptions<SmtpSettings> smtpSettings,
            IOptions<BaaFeatureFlags> baaFeatureFlags,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _smtpSettings = smtpSettings.Value;
            _baaFeatureFlags = baaFeatureFlags.Value;
            _logger = logger;
            _retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    DelayGenerator = static args => new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(1000 * args.AttemptNumber)),
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .Build();
        }

        public async Task SendEmailAsync(Message message)
        {
            if (IsPhiContainingMessage(message) && !_baaFeatureFlags.SmtpBaaExecuted)
            {
                _logger.LogWarning(
                    "PHI email suppressed: BAA not executed for SMTP provider. Subject: {Subject}",
                    message.Subject);
                return;
            }

            var emailUsername = _configuration["EmailSettings:EmailUsername"]
                ?? throw new InvalidOperationException("EmailSettings:EmailUsername is not configured.");
            var emailPassword = _configuration["EmailSettings:EmailPassword"]
                ?? throw new InvalidOperationException("EmailSettings:EmailPassword is not configured.");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Medical Center", emailUsername));
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart("html") { Text = message.Body };
            foreach (var recipient in message.To)
                emailMessage.To.Add(MailboxAddress.Parse(recipient));

            await _retryPipeline.ExecuteAsync(async _ =>
            {
                using var smtpClient = new SmtpClient();
                var secureSocketOptions = _smtpSettings.UseTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                await smtpClient.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, secureSocketOptions);
                await smtpClient.AuthenticateAsync(emailUsername, emailPassword);
                await smtpClient.SendAsync(emailMessage);
                await smtpClient.DisconnectAsync(true);
            });
        }

        private static bool IsPhiContainingMessage(Message message)
        {
            return message.Subject.Contains("Appointment Confirmation", StringComparison.OrdinalIgnoreCase)
                || message.Subject.Contains("Breach", StringComparison.OrdinalIgnoreCase);
        }
    }
}
