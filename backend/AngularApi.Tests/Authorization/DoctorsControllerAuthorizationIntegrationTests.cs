using System.Net;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class DoctorsControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public DoctorsControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDoctors_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Doctors");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDoctors_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");

        var response = await client.GetAsync("/api/Doctors");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDoctorBookings_WhenDoctorIdDoesNotMatchUser_ReturnsForbidden()
    {
        var client = CreateClientForUser("doctor-a", "doctor");

        var response = await client.GetAsync("/api/Doctors/doctor-b/bookings");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDoctorBookings_AsAdminForAnyDoctor_ReturnsSuccess()
    {
        await SeedDoctorAsync("doctor-b");

        var client = CreateClientForUser("admin-user", "admin");

        var response = await client.GetAsync("/api/Doctors/doctor-b/bookings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
