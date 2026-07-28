using System.Net;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Filters;

public class NoCachePhiActionFilterTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public NoCachePhiActionFilterTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthenticatedEndpoint_IncludesNoCacheHeaders()
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

        var response = await client.GetAsync("/api/Account/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl.ToString().Should().Contain("no-store");
        response.Headers.Pragma.ToString().Should().Contain("no-cache");
    }

    [Fact]
    public async Task AnonymousEndpoint_DoesNotIncludeNoCacheHeaders()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("Cache-Control").Should().BeFalse();
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
