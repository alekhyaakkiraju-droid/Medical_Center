using System.Net;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class MedicalCenterDoctorAvailabilitiesControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public MedicalCenterDoctorAvailabilitiesControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMedicalCenterDoctorAvailabilities_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/MedicalCenterDoctorAvailabilities");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMedicalCenterDoctorAvailabilities_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");

        var response = await client.GetAsync("/api/MedicalCenterDoctorAvailabilities");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMedicalCenterDoctorAvailabilities_WithAdminRole_ReturnsSuccess()
    {
        var client = CreateClientWithRole("admin");

        var response = await client.GetAsync("/api/MedicalCenterDoctorAvailabilities");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
