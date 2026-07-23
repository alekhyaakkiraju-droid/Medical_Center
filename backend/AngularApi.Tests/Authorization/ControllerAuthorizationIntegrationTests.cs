using System.Net;
using System.Net.Http.Headers;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Authorization;

public class ControllerAuthorizationIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public ControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllAppointments_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllAppointments_WithUserRole_ReturnsForbidden()
    {
        var client = CreateAuthenticatedClient("user");

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllAppointments_WithAdminRole_ReturnsSuccess()
    {
        var client = CreateAuthenticatedClient("admin");

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPatients_WithUserRole_ReturnsForbidden()
    {
        var client = CreateAuthenticatedClient("user");

        var response = await client.GetAsync("/api/Patients");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDoctorsWithSpectialization_WithoutAuthentication_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/DoctorsWithSpectialization");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDoctors_WithUserRole_ReturnsForbidden()
    {
        var client = CreateAuthenticatedClient("user");

        var response = await client.GetAsync("/api/Doctors");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private HttpClient CreateAuthenticatedClient(string role)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var token = TestJwtFactory.CreateToken(configuration, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}
