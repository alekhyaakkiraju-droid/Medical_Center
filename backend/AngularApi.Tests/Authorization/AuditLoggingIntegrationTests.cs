using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
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

        var payload = new
        {
            doctorId = "doctor1",
            appointmentTakenDate = DateTime.UtcNow
        };

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
    }
}
