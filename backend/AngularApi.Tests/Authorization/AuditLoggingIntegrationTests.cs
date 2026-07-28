using AngularApi.Models;
using AngularApi.Contracts.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AngularApi.Tests.Authorization;

public class AuditLoggingIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public AuditLoggingIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostAppointment_CreatesAuditLogRecord()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roleManager.EnsureRolesCreatedAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

            context.Doctors.Add(new Doctor { Id = "doctor1", Name = "Dr. Smith" });

            var patient = new Patient
            {
                Id = "patient1",
                UserName = "patient1@example.com",
                Email = "patient1@example.com",
                EmailConfirmed = true,
            };
            var createResult = await userManager.CreateAsync(patient, "Password123!");
            createResult.Succeeded.Should().BeTrue();
            await userManager.AddToRoleAsync(patient, "user");
            await context.SaveChangesAsync();
        }

        var client = AntiforgeryTestHelper.CreateClient(_factory);
        var token = TestJwtFactory.CreateTokenForUser(
            _factory.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
            "patient1",
            "user");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var payload = AppointmentTestPayloads.Valid("doctor1");

        var response = await client.PostAsJsonAsync("/api/Appointments", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var queryScope = _factory.Services.CreateScope();
        var queryContext = queryScope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var auditLog = await queryContext.AuditLogs
            .OrderByDescending(log => log.Id)
            .FirstOrDefaultAsync(log => log.EntityType == "Appointment");

        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("POST");
        auditLog.Actor.Should().Be("patient1");
        auditLog.EntityType.Should().Be("Appointment");
    }

    [Fact]
    public async Task GetProtectedEndpoint_DoesNotCreateAuditLogEntry()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roleManager.EnsureRolesCreatedAsync();
        }

        int countBefore;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
            countBefore = await context.AuditLogs.CountAsync();
        }

        var client = _factory.CreateClient();
        var token = TestJwtFactory.CreateToken(
            _factory.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
            "admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var queryScope = _factory.Services.CreateScope();
        var queryContext = queryScope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var countAfter = await queryContext.AuditLogs.CountAsync();
        countAfter.Should().Be(countBefore);
    }

    [Fact]
    public async Task AuditLogEntry_CannotBeModifiedOrDeleted()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        context.AuditLogs.Add(new AuditLog
        {
            Actor = "append-only-user",
            Action = "POST",
            EntityType = "AppendOnlyTest",
            Timestamp = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var auditLog = await context.AuditLogs.SingleAsync(log => log.EntityType == "AppendOnlyTest");
        auditLog.Action = "PUT";

        var modifyAct = async () => await context.SaveChangesAsync();
        await modifyAct.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");

        using var deleteScope = _factory.Services.CreateScope();
        var deleteContext = deleteScope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var deleteTarget = await deleteContext.AuditLogs.SingleAsync(log => log.EntityType == "AppendOnlyTest");
        deleteContext.AuditLogs.Remove(deleteTarget);

        var deleteAct = async () => await deleteContext.SaveChangesAsync();
        await deleteAct.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }
}
