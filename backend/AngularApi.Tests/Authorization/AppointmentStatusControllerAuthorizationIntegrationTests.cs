using System.Net;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class AppointmentStatusControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public AppointmentStatusControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAppointmentStatus_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/AppointmentStatus");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAppointmentStatus_WithDoctorRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("doctor");

        var response = await client.GetAsync("/api/AppointmentStatus");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentStatus_WithAdminRole_ReturnsSuccess()
    {
        var client = CreateClientWithRole("admin");

        var response = await client.GetAsync("/api/AppointmentStatus");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
