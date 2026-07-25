using System.Net;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class AppointmentsControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public AppointmentsControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAllAppointments_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllAppointments_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentsByDate_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");

        var response = await client.GetAsync("/api/Appointments/date/2026-01-01");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAppointment_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.DeleteAsync("/api/Appointments/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllAppointments_WithAdminRole_ReturnsSuccess()
    {
        var client = CreateClientWithRole("admin");

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
