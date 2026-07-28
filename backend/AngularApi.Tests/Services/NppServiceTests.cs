using AngularApi.Contracts.Models;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace AngularApi.Tests.Services;

public class NppServiceTests
{
    [Fact]
    public async Task GetStatusAsync_WithNoAcknowledgment_ReturnsFalse()
    {
        var service = CreateService("1.0");

        var status = await service.GetStatusAsync("user-1");

        status.Acknowledged.Should().BeFalse();
        status.Version.Should().Be("1.0");
    }

    [Fact]
    public async Task GetStatusAsync_WithMatchingVersion_ReturnsTrue()
    {
        var context = CreateContext();
        context.AuditLogs.Add(new AuditLog
        {
            Actor = "user-1",
            Action = "Acknowledge",
            EntityType = "NPPAcknowledgment",
            EntityId = "user-1",
            NewValues = "{\"version\":\"1.0\",\"timestamp\":\"2026-01-01T00:00:00Z\"}",
        });
        await context.SaveChangesAsync();

        var service = CreateService("1.0", context);
        var status = await service.GetStatusAsync("user-1");

        status.Acknowledged.Should().BeTrue();
        status.AcknowledgedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatusAsync_WithOutdatedVersion_ReturnsFalse()
    {
        var context = CreateContext();
        context.AuditLogs.Add(new AuditLog
        {
            Actor = "user-1",
            Action = "Acknowledge",
            EntityType = "NPPAcknowledgment",
            EntityId = "user-1",
            NewValues = "{\"version\":\"1.0\",\"timestamp\":\"2026-01-01T00:00:00Z\"}",
        });
        await context.SaveChangesAsync();

        var service = CreateService("1.1", context);
        var status = await service.GetStatusAsync("user-1");

        status.Acknowledged.Should().BeFalse();
        status.Version.Should().Be("1.1");
    }

    [Fact]
    public async Task AcknowledgeAsync_CreatesAuditLogEntry()
    {
        var context = CreateContext();
        var service = CreateService("1.0", context);

        await service.AcknowledgeAsync("user-1");

        var entry = await context.AuditLogs.SingleAsync();
        entry.EntityType.Should().Be("NPPAcknowledgment");
        entry.Action.Should().Be("Acknowledge");
        entry.Actor.Should().Be("user-1");
        entry.NewValues.Should().Contain("\"version\":\"1.0\"");
    }

    private static NppService CreateService(string version, MedicalCenterDbContext? context = null)
    {
        context ??= CreateContext();
        var settings = Microsoft.Extensions.Options.Options.Create(new NppSettings
        {
            CurrentVersion = version,
            ContentFilePath = "wwwroot/legal/npp.md",
        });

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(env => env.ContentRootPath).Returns(Directory.GetCurrentDirectory());

        return new NppService(context, settings, environment.Object);
    }

    private static MedicalCenterDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MedicalCenterDbContext(options);
    }
}
