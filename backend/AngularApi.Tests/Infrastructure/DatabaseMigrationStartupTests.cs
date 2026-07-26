using AngularApi.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AngularApi.Tests.Infrastructure;

public class DatabaseMigrationStartupTests : IDisposable
{
    private readonly Mock<IDatabaseMigrationRunner> _migratorMock = new();
    private readonly List<LogEntry> _logEntries = [];
    private int? _exitCode;

    public DatabaseMigrationStartupTests()
    {
        DatabaseMigrationStartup.ExitApplication = code =>
        {
            _exitCode = code;
            throw new InvalidOperationException($"Startup failed with exit code {code}");
        };
    }

    public void Dispose()
    {
        DatabaseMigrationStartup.ExitApplication = Environment.Exit;
    }

    [Fact]
    public async Task ApplyPendingMigrationsAsync_CallsMigrateAsync()
    {
        _migratorMock
            .Setup(m => m.ApplyPendingMigrationsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await DatabaseMigrationStartup.ApplyPendingMigrationsAsync(CreateServiceProvider());

        _migratorMock.Verify(
            m => m.ApplyPendingMigrationsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _logEntries.Should().Contain(entry =>
            entry.Level == LogLevel.Information &&
            entry.Message.Contains("Database migrations applied successfully", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ApplyPendingMigrationsAsync_LogsCriticalAndExits_WhenMigrationFails()
    {
        var migrationException = new InvalidOperationException("Migration failed");
        _migratorMock
            .Setup(m => m.ApplyPendingMigrationsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(migrationException);

        var act = () => DatabaseMigrationStartup.ApplyPendingMigrationsAsync(CreateServiceProvider());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Startup failed with exit code 1*");
        _exitCode.Should().Be(1);
        _logEntries.Should().Contain(entry =>
            entry.Level == LogLevel.Critical &&
            entry.Exception == migrationException);
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_migratorMock.Object);
        services.AddLogging(builder =>
        {
            builder.AddProvider(new ListLoggerProvider(_logEntries));
        });
        return services.BuildServiceProvider();
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class ListLoggerProvider(List<LogEntry> entries) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new ListLogger(entries);

        public void Dispose()
        {
        }
    }

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
            entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }
}
