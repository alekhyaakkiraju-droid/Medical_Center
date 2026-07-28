using System.Net;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Authorization;

public class SessionTimeoutIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public SessionTimeoutIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SessionTimeout_Returns200_ClearsCookies_AndRecordsAuditLog()
    {
        await SeedUserAsync();
        var client = AntiforgeryTestHelper.CreateClient(_factory);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var loginResponse = await client.PostAsJsonAsync("/api/Account/login", new LogInUserDTO
        {
            Email = SeedData.TestUserEmail,
            Password = SeedData.TestUserPassword,
        });
        loginResponse.EnsureSuccessStatusCode();
        AntiforgeryTestHelper.ImportAuthCookies(loginResponse, client);

        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        var response = await client.PostAsync("/api/Account/session-timeout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();
        string.Join("; ", setCookieHeaders!).Should().Contain("MedCenter.Auth=");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var auditEntry = await dbContext.AuditLogs
            .OrderByDescending(entry => entry.Timestamp)
            .FirstOrDefaultAsync(entry => entry.Action == "SessionTimeout");

        auditEntry.Should().NotBeNull();
        auditEntry!.Actor.Should().Be(SeedData.TestUserEmail);
    }

    private async Task SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.EnsureRolesCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(SeedData.TestUserEmail) != null)
        {
            return;
        }

        var user = new AppUser
        {
            UserName = SeedData.TestUserEmail,
            Email = SeedData.TestUserEmail,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, SeedData.TestUserPassword);
        result.Succeeded.Should().BeTrue();
        await userManager.AddToRoleAsync(user, "user");
    }
}
