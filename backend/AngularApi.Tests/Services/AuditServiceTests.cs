using AngularApi.Models;
using AngularApi.Services.impelementation;
using AngularApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AngularApi.Tests.Services;

public class AuditServiceTests
{
    [Fact]
    public async Task RecordAsync_PersistsAuditLogEntry()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new MedicalCenterDbContext(options);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(accessor => accessor.HttpContext).Returns(new DefaultHttpContext());

        var service = new AuditService(context, httpContextAccessor.Object);

        await service.RecordAsync("POST", "Appointment", "1", newValues: "{\"doctorId\":\"doctor1\"}");

        var auditLog = await context.AuditLogs.SingleAsync();
        auditLog.Action.Should().Be("POST");
        auditLog.EntityType.Should().Be("Appointment");
        auditLog.NewValues.Should().Contain("doctor1");
    }

    [Fact]
    public async Task RecordAuthEventAsync_StoresAuthenticationEntityType()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new MedicalCenterDbContext(options);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var service = new AuditService(context, httpContextAccessor.Object);

        await service.RecordAuthEventAsync("LoginSuccess", "user@example.com", true);

        var auditLog = await context.AuditLogs.SingleAsync();
        auditLog.EntityType.Should().Be("Authentication");
        auditLog.Action.Should().Be("LoginSuccess");
        auditLog.NewValues.Should().Be("Succeeded");
    }
}
