using System.Net;
using System.Net.Http.Json;
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
        const string email = "npp-unacknowledged@example.com";
        await SeedUserAsync(email);
        var client = await CreateAuthenticatedClientAsync(email);

        var response = await client.GetAsync("/api/npp/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<NppStatusResponse>();
        body!.Acknowledged.Should().BeFalse();
        body.Version.Should().Be("1.0");
    }

    [Fact]
    public async Task Acknowledge_ThenGetStatus_ReturnsTrue()
    {
        const string email = "npp-acknowledged@example.com";
        await SeedUserAsync(email);
        var client = await CreateAuthenticatedClientAsync(email);

        var acknowledgeResponse = await client.PostAsync("/api/npp/acknowledge", null);
        acknowledgeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusResponse = await client.GetAsync("/api/npp/status");
        var body = await statusResponse.Content.ReadFromJsonAsync<NppStatusResponse>();
        body!.Acknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task GetContent_ReturnsVersionAndContent()
    {
        const string email = "npp-content@example.com";
        await SeedUserAsync(email);
        var client = await CreateAuthenticatedClientAsync(email);

        var response = await client.GetAsync("/api/npp/content");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<NppContentResponse>();
        body!.Version.Should().Be("1.0");
        body.Content.Should().Contain("Notice of Privacy Practices");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var client = AntiforgeryTestHelper.CreateClient(_factory);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var loginResponse = await client.PostAsJsonAsync("/api/Account/login", new LogInUserDTO
        {
            Email = email,
            Password = SeedData.TestUserPassword,
        });
        loginResponse.EnsureSuccessStatusCode();
        AntiforgeryTestHelper.ImportAuthCookies(loginResponse, client);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        return client;
    }

    private async Task SeedUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.EnsureRolesCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(email) != null)
        {
            return;
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, SeedData.TestUserPassword);
        result.Succeeded.Should().BeTrue();
        await userManager.AddToRoleAsync(user, "user");
    }
}
