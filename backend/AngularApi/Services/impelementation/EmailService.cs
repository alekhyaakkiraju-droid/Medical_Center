using AngularApi.Contracts.Services;
using AngularApi.Options;
using AngularApi.Contracts.Services.Interfaces;
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
        private readonly IEmailTransport _emailTransport;
        private readonly ILogger<EmailService> _logger;
        private readonly ResiliencePipeline _retryPipeline;

        public EmailService(
            IConfiguration configuration,
            IOptions<SmtpSettings> smtpSettings,
            IEmailTransport emailTransport,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _smtpSettings = smtpSettings.Value;
            _emailTransport = emailTransport;
            _logger = logger;
            _retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    DelayGenerator = static args =>
                        new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(1000 * args.AttemptNumber)),
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    OnRetry = args =>
                    {
                        logger.LogWarning(
                            "Email SMTP send retry attempt {AttemptNumber} after {Delay}ms delay",
                            args.AttemptNumber,
                            1000 * args.AttemptNumber);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
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

            await _retryPipeline.ExecuteAsync(
                async token => await _emailTransport.SendAsync(
                    emailMessage,
                    _smtpSettings,
                    emailUsername,
                    emailPassword,
                    token));
        }
    }
}
