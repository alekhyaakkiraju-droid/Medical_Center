using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Authorization;

public class CookieAuthIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public CookieAuthIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_SetsSecureHttpOnlyAuthCookie_AndOmitsTokenFromBody()
    {
        await SeedUserAsync();
        var client = CreateCookieAuthClient("203.0.113.50");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Account/login", new LogInUserDTO
        {
            Email = SeedData.TestUserEmail,
            Password = SeedData.TestUserPassword,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();

        var combinedCookies = string.Join("; ", setCookieHeaders!);
        combinedCookies.Should().Contain("MedCenter.Auth=");
        combinedCookies.Should().Contain("httponly", because: "auth cookie must be HttpOnly");
        combinedCookies.Should().Contain("samesite=strict", because: "auth cookie must use SameSite=Strict");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("token");
        body.Should().Contain("expiration");
    }

    [Fact]
    public async Task RefreshToken_RotatesAuthCookies()
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
        var refreshResponse = await client.PostAsync("/api/Account/refresh-token", null);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        refreshResponse.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();
        string.Join("; ", setCookieHeaders!).Should().Contain("MedCenter.Auth=");
    }

    [Fact]
    public async Task CookieAuth_AllowsAccessToProtectedEndpoint()
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

        var response = await client.GetAsync("/api/Account/user-details");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Me_ReturnsAuthenticatedUserProfile()
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
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(SeedData.TestUserEmail);
        body.Should().Contain("roles");
    }

    [Fact]
    public async Task Logout_ClearsAuthCookiesAndBlocksProtectedEndpoint()
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
        var logoutResponse = await client.PostAsync("/api/Account/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        logoutResponse.Headers.TryGetValues("Set-Cookie", out var logoutCookies).Should().BeTrue();
        string.Join("; ", logoutCookies!).Should().Contain("MedCenter.Auth=");

        client.DefaultRequestHeaders.Remove("Cookie");

        var meResponse = await client.GetAsync("/api/Account/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_RejectsPreviouslyRotatedRefreshToken()
    {
        await SeedUserAsync();
        var client = CreateCookieAuthClient("203.0.113.51");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var loginResponse = await client.PostAsJsonAsync("/api/Account/login", new LogInUserDTO
        {
            Email = SeedData.TestUserEmail,
            Password = SeedData.TestUserPassword,
        });
        loginResponse.EnsureSuccessStatusCode();
        loginResponse.Headers.TryGetValues("Set-Cookie", out var loginCookies).Should().BeTrue();

        var originalAuthCookie = loginCookies!.First(cookie => cookie.StartsWith("MedCenter.Auth=", StringComparison.Ordinal))
            .Split(';')[0];
        var originalRefreshCookie = loginCookies.First(cookie => cookie.StartsWith("MedCenter.Refresh=", StringComparison.Ordinal))
            .Split(';')[0];

        AntiforgeryTestHelper.ImportAuthCookies(loginResponse, client);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var refreshResponse = await client.PostAsync("/api/Account/refresh-token", null);
        refreshResponse.EnsureSuccessStatusCode();

        var replayClient = CreateCookieAuthClient("203.0.113.52");
        replayClient.DefaultRequestHeaders.Remove("Cookie");
        replayClient.DefaultRequestHeaders.Add("Cookie", $"{originalAuthCookie}; {originalRefreshCookie}");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(replayClient);

        var replayResponse = await replayClient.PostAsync("/api/Account/refresh-token", null);
        replayResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExpiredJwt_ReturnsUnauthorizedOnProtectedEndpoint()
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var token = TestJwtFactory.CreateExpiredTokenForUser(configuration, "expired-user", "user");

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/Account/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

        var passwordValid = await userManager.CheckPasswordAsync(user, SeedData.TestUserPassword);
        passwordValid.Should().BeTrue();
    }

    private HttpClient CreateCookieAuthClient(string ipAddress)
    {
        var client = AntiforgeryTestHelper.CreateClient(_factory);
        client.DefaultRequestHeaders.Add("X-Test-Client-Ip", ipAddress);
        return client;
    }
}
