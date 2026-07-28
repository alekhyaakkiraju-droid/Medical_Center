using AngularApi.Contracts.Services;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;

namespace AngularApi.Tests.Services;

public class EmailServiceRetryTests
{
    [Fact]
    public async Task SendEmailAsync_RetriesTransientFailuresAndEventuallySucceeds()
    {
        var transport = new Mock<IEmailTransport>();
        var attempts = 0;
        transport
            .Setup(t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attempts++;
                if (attempts < 3)
                {
                    throw new InvalidOperationException("Transient SMTP failure");
                }

                return Task.CompletedTask;
            });

        var emailService = CreateEmailService(transport.Object);

        await emailService.SendEmailAsync(new Message(["recipient@example.com"], "Subject", "Body"));

        attempts.Should().Be(3);
        transport.Verify(
            t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task SendEmailAsync_PropagatesExceptionAfterExhaustingRetries()
    {
        var transport = new Mock<IEmailTransport>();
        transport
            .Setup(t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Persistent SMTP failure"));

        var emailService = CreateEmailService(transport.Object);

        var act = () => emailService.SendEmailAsync(new Message(["recipient@example.com"], "Subject", "Body"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Persistent SMTP failure");

        transport.Verify(
            t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(4));
    }

    private static EmailService CreateEmailService(IEmailTransport transport)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:EmailUsername"] = "test@example.com",
                ["EmailSettings:EmailPassword"] = "test-password",
            })
            .Build();

        var smtpSettings = Microsoft.Extensions.Options.Options.Create(new SmtpSettings
        {
            Host = "localhost",
            Port = 1025,
            UseTls = false,
        });

        return new EmailService(
            configuration,
            smtpSettings,
            transport,
            Microsoft.Extensions.Options.Options.Create(new BaaFeatureFlags { SmtpBaaExecuted = true }),
            NullLogger<EmailService>.Instance);
    }
}
