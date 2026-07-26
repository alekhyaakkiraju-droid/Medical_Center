using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Services.impelementation;
using AngularApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace AngularApi.Tests.Services;

public class BreachNotificationServiceTests
{
    private static BreachNotificationService CreateService(
        MedicalCenterDbContext context,
        Mock<IEmailService>? emailServiceMock = null,
        BreachDetectionOptions? options = null)
    {
        var auditService = new AuditService(context, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>());
        emailServiceMock ??= new Mock<IEmailService>();
        emailServiceMock.Setup(s => s.SendEmailAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        var webHostMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        var templateDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "EmailTemplates");
        Directory.CreateDirectory(templateDir);
        File.WriteAllText(Path.Combine(templateDir, "BreachNotification.html"),
            "{{AffectedIndividualName}}{{BreachDescription}}{{DateDiscovered}}{{RecommendedActions}}{{ContactInformation}}");
        webHostMock.Setup(e => e.WebRootPath).Returns(Path.GetDirectoryName(templateDir)!);

        var emailTemplateService = new EmailTemplateService(webHostMock.Object);
        var breachOptions = Microsoft.Extensions.Options.Options.Create(options ?? new BreachDetectionOptions
        {
            FailedAuthThreshold = 15,
            MutationThreshold = 50,
            WindowMinutes = 5
        });

        return new BreachNotificationService(
            context,
            auditService,
            emailServiceMock.Object,
            emailTemplateService,
            breachOptions);
    }

    private static async Task<MedicalCenterDbContext> CreateContextWithLogs(IEnumerable<AuditLog> logs)
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new MedicalCenterDbContext(options);
        context.AuditLogs.AddRange(logs);
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task DetectAnomaliesAsync_WithNormalPattern_ReturnsEmpty()
    {
        var now = DateTime.UtcNow;
        await using var context = await CreateContextWithLogs([
            new AuditLog { Actor = "user1", Timestamp = now.AddMinutes(-1), Action = "LoginFailed", EntityType = "Authentication", NewValues = "Failed" },
            new AuditLog { Actor = "user1", Timestamp = now.AddMinutes(-2), Action = "POST", EntityType = "Patient", NewValues = "{}" }
        ]);

        var service = CreateService(context);
        var anomalies = await service.DetectAnomaliesAsync();

        anomalies.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAnomaliesAsync_WithFailedAuthSpike_ReturnsAnomaly()
    {
        var now = DateTime.UtcNow;
        var logs = Enumerable.Range(0, 20)
            .Select(i => new AuditLog
            {
                Actor = "attacker@example.com",
                Timestamp = now.AddMinutes(-1).AddSeconds(-i),
                Action = "LoginFailed",
                EntityType = "Authentication",
                NewValues = "Failed"
            })
            .Cast<AuditLog>()
            .ToList();

        await using var context = await CreateContextWithLogs(logs);
        var service = CreateService(context, options: new BreachDetectionOptions { FailedAuthThreshold = 15, WindowMinutes = 5, MutationThreshold = 50 });

        var anomalies = await service.DetectAnomaliesAsync();

        anomalies.Should().ContainSingle(a => a.AnomalyType == "FailedAuthenticationSpike");
        anomalies.Single().EventCount.Should().Be(20);
        anomalies.Single().Actor.Should().Be("attacker@example.com");
    }

    [Fact]
    public async Task DetectAnomaliesAsync_WithUnusualDataAccessVolume_ReturnsAnomaly()
    {
        var now = DateTime.UtcNow;
        var logs = Enumerable.Range(0, 55)
            .Select(i => new AuditLog
            {
                Actor = "doctor1",
                Timestamp = now.AddMinutes(-1).AddSeconds(-i),
                Action = "POST",
                EntityType = "Appointment",
                NewValues = "{}"
            })
            .Cast<AuditLog>()
            .ToList();

        await using var context = await CreateContextWithLogs(logs);
        var service = CreateService(context, options: new BreachDetectionOptions { FailedAuthThreshold = 15, WindowMinutes = 5, MutationThreshold = 50 });

        var anomalies = await service.DetectAnomaliesAsync();

        anomalies.Should().ContainSingle(a => a.AnomalyType == "UnusualDataAccessVolume");
        anomalies.Single().EventCount.Should().Be(55);
    }

    [Fact]
    public async Task DetectAnomaliesAsync_WithEmptyAuditLog_ReturnsEmpty()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new MedicalCenterDbContext(options);

        var service = CreateService(context);
        var anomalies = await service.DetectAnomaliesAsync();

        anomalies.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAnomaliesAsync_AtThresholdBoundary_DoesNotReturnAnomaly()
    {
        var now = DateTime.UtcNow;
        var logs = Enumerable.Range(0, 14)
            .Select(i => new AuditLog
            {
                Actor = "user@example.com",
                Timestamp = now.AddMinutes(-1).AddSeconds(-i),
                Action = "LoginFailed",
                EntityType = "Authentication",
                NewValues = "Failed"
            })
            .Cast<AuditLog>()
            .ToList();

        await using var context = await CreateContextWithLogs(logs);
        var service = CreateService(context, options: new BreachDetectionOptions { FailedAuthThreshold = 15, WindowMinutes = 5, MutationThreshold = 50 });

        var anomalies = await service.DetectAnomaliesAsync();

        anomalies.Should().BeEmpty();
    }

    [Fact]
    public async Task AssessBreachAsync_LogsAssessmentWithoutNotifications()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new MedicalCenterDbContext(options);
        var service = CreateService(context);

        var assessment = new BreachAssessmentDTO
        {
            Description = "Suspicious access pattern detected",
            AffectedEntityTypes = ["Patient"],
            DiscoveryDate = DateTime.UtcNow.AddHours(-1),
            SeverityLevel = "High"
        };

        var result = await service.AssessBreachAsync(assessment);

        result.Status.Should().Be("Assessed");
        result.NotificationsSent.Should().Be(0);
        var auditLog = await context.AuditLogs.SingleAsync();
        auditLog.Action.Should().Be("BreachAssessment");
        auditLog.EntityType.Should().Be("BreachAssessment");
    }
}
