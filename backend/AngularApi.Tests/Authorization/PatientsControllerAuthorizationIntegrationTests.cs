using System.Net;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class PatientsControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public PatientsControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetPatients_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Patients");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatients_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");

        var response = await client.GetAsync("/api/Patients");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPatientById_WhenPatientIdDoesNotMatchUser_ReturnsForbidden()
    {
        await SeedPatientAsync("patient-b");

        var client = CreateClientForUser("patient-a", "user");

        var response = await client.GetAsync("/api/Patients/patient-b");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPatientById_AsAdminForAnyPatient_ReturnsSuccess()
    {
        await SeedPatientAsync("patient-b");

        var client = CreateClientForUser("admin-user", "admin");

        var response = await client.GetAsync("/api/Patients/patient-b");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
