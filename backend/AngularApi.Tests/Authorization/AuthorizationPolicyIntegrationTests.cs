using System.Net;
using System.Net.Http.Json;
using AngularApi.DTO;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AngularApi.Tests.Authorization;

public class AuthorizationPolicyIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizationPolicyIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAppointments_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Appointments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostLogin_WithInvalidModel_AllowsAnonymousAccess()
    {
        var response = await _client.PostAsJsonAsync("/api/Account/login", new LogInUserDTO());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSpecializations_WithoutAuthentication_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/Specializations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
