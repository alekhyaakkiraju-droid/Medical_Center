using System.Net;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Controllers;

public class NppControllerTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public NppControllerTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetStatus_WhenUnacknowledged_ReturnsFalse()
    {
        await SeedUserAsync();
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/npp/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<NppStatusResponse>();
        body!.Acknowledged.Should().BeFalse();
        body.Version.Should().Be("1.0");
    }

    [Fact]
    public async Task Acknowledge_ThenGetStatus_ReturnsTrue()
    {
        await SeedUserAsync();
        var client = await CreateAuthenticatedClientAsync();

        var acknowledgeResponse = await client.PostAsync("/api/npp/acknowledge", null);
        acknowledgeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusResponse = await client.GetAsync("/api/npp/status");
        var body = await statusResponse.Content.ReadFromJsonAsync<NppStatusResponse>();
        body!.Acknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task GetContent_ReturnsVersionAndContent()
    {
        await SeedUserAsync();
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/npp/content");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<NppContentResponse>();
        body!.Version.Should().Be("1.0");
        body.Content.Should().Contain("Notice of Privacy Practices");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = AntiforgeryTestHelper.CreateClient(_factory);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var loginResponse = await client.PostAsJsonAsync("/api/Account/login", new LogInUserDTO
        {
            Email = SeedData.TestUserEmail,
            Password = SeedData.TestUserPassword,
        });
        loginResponse.EnsureSuccessStatusCode();
        AntiforgeryTestHelper.ImportAuthCookies(loginResponse, client);
        return client;
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
