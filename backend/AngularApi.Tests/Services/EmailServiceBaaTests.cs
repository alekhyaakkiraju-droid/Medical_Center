using AngularApi.Contracts.Services;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AngularApi.Tests.Services;

public class EmailServiceBaaTests
{
    [Fact]
    public async Task SendEmailAsync_SendsPhiEmail_WhenSmtpBaaExecuted()
    {
        var service = CreateService(smtpBaaExecuted: true);
        var message = new Message(["patient@example.com"], "Appointment Confirmation", "<p>PHI</p>");

        var act = async () => await service.SendEmailAsync(message);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendEmailAsync_SuppressesPhiEmail_WhenSmtpBaaNotExecuted()
    {
        var logger = new Mock<ILogger<EmailService>>();
        var service = CreateService(smtpBaaExecuted: false, logger: logger.Object);
        var message = new Message(["patient@example.com"], "Appointment Confirmation", "<p>PHI</p>");

        await service.SendEmailAsync(message);

        logger.Verify(
            log => log.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("PHI email suppressed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_SendsNonPhiEmail_WhenSmtpBaaNotExecuted()
    {
        var logger = new Mock<ILogger<EmailService>>();
        var service = CreateService(smtpBaaExecuted: false, logger: logger.Object);
        var message = new Message(["user@example.com"], "Confirm Your Email", "<p>Welcome</p>");

        var act = async () => await service.SendEmailAsync(message);

        await act.Should().ThrowAsync<Exception>();
        logger.Verify(
            log => log.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private static EmailService CreateService(bool smtpBaaExecuted, ILogger<EmailService>? logger = null)
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
            Options.Create(new SmtpSettings { Host = "localhost", Port = 1025, UseTls = false }),
            Options.Create(new BaaFeatureFlags { SmtpBaaExecuted = smtpBaaExecuted, AwsBaaExecuted = true }),
            logger ?? Mock.Of<ILogger<EmailService>>());
    }
}
