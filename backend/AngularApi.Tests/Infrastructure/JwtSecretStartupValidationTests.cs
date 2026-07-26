using AngularApi.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AngularApi.Tests.Infrastructure;

public class JwtSecretStartupValidationTests
{
    [Fact]
    public void Validate_DoesNotThrow_WhenJwtSecretIsConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [JwtSecretStartupValidation.ConfigurationKey] = "integration-test-secret-key-32chars!",
            })
            .Build();

        var act = () => JwtSecretStartupValidation.Validate(configuration, NullLogger.Instance);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WhenJwtSecretIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var act = () => JwtSecretStartupValidation.Validate(configuration, NullLogger.Instance);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Secret is not configured*")
            .WithMessage("*appsettings*")
            .WithMessage("*/run/secrets/jwt_secret*")
            .WithMessage("*JWT_SECRET_FILE*")
            .WithMessage("*Jwt__Secret*");
    }

    [Fact]
    public void Validate_ThrowsInvalidOperationException_WhenJwtSecretIsWhitespace()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [JwtSecretStartupValidation.ConfigurationKey] = "   ",
            })
            .Build();

        var act = () => JwtSecretStartupValidation.Validate(configuration, NullLogger.Instance);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Secret is not configured*");
    }

    [Fact]
    public void Validate_LogsWarning_WhenJwtSecretIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();
        var logEntries = new List<LogEntry>();

        var act = () => JwtSecretStartupValidation.Validate(
            configuration,
            new ListLogger(logEntries));

        act.Should().Throw<InvalidOperationException>();
        logEntries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Warning &&
            entry.Message.Contains("Jwt:Secret is not configured", StringComparison.Ordinal));
    }

    private sealed record LogEntry(LogLevel Level, string Message);

    private sealed class ListLogger(List<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }
}
