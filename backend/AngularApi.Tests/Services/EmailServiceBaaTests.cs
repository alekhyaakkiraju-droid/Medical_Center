using AngularApi.Contracts.Services;
using AngularApi.Contracts.Services.Interfaces;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;

namespace AngularApi.Tests.Services;

public class EmailServiceBaaTests
{
    [Fact]
    public async Task SendEmailAsync_SendsPhiEmail_WhenSmtpBaaExecuted()
    {
        var transport = new Mock<IEmailTransport>();
        transport
            .Setup(t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var service = CreateService(smtpBaaExecuted: true, transport: transport.Object);

        var act = async () => await service.SendEmailAsync(
            new Message(["patient@example.com"], "Appointment Confirmation", "<p>PHI</p>"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        transport.Verify(
            t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendEmailAsync_SuppressesPhiEmail_WhenSmtpBaaNotExecuted()
    {
        var transport = new Mock<IEmailTransport>();
        var logger = new Mock<ILogger<EmailService>>();
        var service = CreateService(smtpBaaExecuted: false, transport: transport.Object, logger: logger.Object);

        await service.SendEmailAsync(new Message(["patient@example.com"], "Appointment Confirmation", "<p>PHI</p>"));

        logger.Verify(
            log => log.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("PHI email suppressed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        transport.Verify(
            t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_SendsNonPhiEmail_WhenSmtpBaaNotExecuted()
    {
        var transport = new Mock<IEmailTransport>();
        transport
            .Setup(t => t.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<SmtpSettings>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var logger = new Mock<ILogger<EmailService>>();
        var service = CreateService(smtpBaaExecuted: false, transport: transport.Object, logger: logger.Object);

        var act = async () => await service.SendEmailAsync(
            new Message(["user@example.com"], "Confirm Your Email", "<p>Welcome</p>"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        logger.Verify(
            log => log.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("PHI email suppressed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private static EmailService CreateService(
        bool smtpBaaExecuted,
        IEmailTransport? transport = null,
        ILogger<EmailService>? logger = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:EmailUsername"] = "test@example.com",
                ["EmailSettings:EmailPassword"] = "password",
            })
            .Build();

        return new EmailService(
            configuration,
            Microsoft.Extensions.Options.Options.Create(new SmtpSettings { Host = "localhost", Port = 1025, UseTls = false }),
            transport ?? Mock.Of<IEmailTransport>(),
            Microsoft.Extensions.Options.Options.Create(new BaaFeatureFlags { SmtpBaaExecuted = smtpBaaExecuted, AwsBaaExecuted = true }),
            logger ?? NullLogger<EmailService>.Instance);
    }
}
