using AngularApi.Models;
using AngularApi.Contracts.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.Models;

public class AuditLogAppendOnlyTests
{
    [Fact]
    public async Task SaveChangesAsync_ThrowsWhenAuditLogIsModified()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new MedicalCenterDbContext(options);

        context.AuditLogs.Add(new AuditLog
        {
            Actor = "user1",
            Action = "POST",
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var auditLog = await context.AuditLogs.SingleAsync();
        auditLog.Action = "PUT";

        var act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }
}
