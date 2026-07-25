using System.Net;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class MedicalCentersControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public MedicalCentersControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMedicalCenters_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/MedicalCenters");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMedicalCenters_WithDoctorRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("doctor");

        var response = await client.GetAsync("/api/MedicalCenters");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMedicalCenters_WithAdminRole_ReturnsSuccess()
    {
        var client = CreateClientWithRole("admin");

        var response = await client.GetAsync("/api/MedicalCenters");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
