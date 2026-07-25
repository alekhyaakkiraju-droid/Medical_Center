using System.Net;
using System.Net.Http.Json;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class SpecializationsControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public SpecializationsControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetSpecializationById_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Specializations/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSpecializationById_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");

        var response = await client.GetAsync("/api/Specializations/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostSpecialization_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Specializations", new
        {
            SpecializationName = "Cardiology",
            Description = "Heart specialist",
            IsActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostSpecialization_WithAdminRole_AllowsAccess()
    {
        var client = CreateClientWithRole("admin");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Specializations", new
        {
            SpecializationName = "Neurology",
            Description = "Brain specialist",
            IsActive = true
        });

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
