using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace AngularApi.Tests.Infrastructure;

public class HealthCheckEndpointTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public HealthCheckEndpointTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOkWithoutAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthReadyEndpoint_ReturnsOkWhenDatabaseIsAvailable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        json.RootElement.GetProperty("checks").EnumerateArray().Should().NotBeEmpty();
    }
}
