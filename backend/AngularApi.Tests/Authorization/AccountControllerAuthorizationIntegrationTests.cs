using System.Net;
using System.Net.Http.Json;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class AccountControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public AccountControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Account/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAdmin_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Account/Register/admin", new
        {
            UserName = "new-admin",
            Email = "new-admin@example.com",
            Password = "SecurePassword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangePassword_WithDoctorRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("doctor");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Account/change-password", new
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegisterAdmin_WithAdminRole_AllowsAccess()
    {
        var client = CreateClientWithRole("admin");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Account/Register/admin", new
        {
            UserName = "another-admin",
            Email = "another-admin@example.com",
            Password = "SecurePassword123!"
        });

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
