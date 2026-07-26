using FluentAssertions;
using System.Net;

namespace AngularApi.Tests.Infrastructure;

public class CorsIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public CorsIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AllowedOrigin_ReceivesCorsHeadersWithCredentials()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:4200");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Single()
            .Should().Be("http://localhost:4200");
        response.Headers.Should().ContainKey("Access-Control-Allow-Credentials");
        response.Headers.GetValues("Access-Control-Allow-Credentials").Single()
            .Should().Be("true");
    }

    [Fact]
    public async Task UnlistedOrigin_DoesNotReceiveAccessControlAllowOriginHeader()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://evil.example");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().NotContainKey("Access-Control-Allow-Origin");
        response.Headers.Should().NotContainKey("Access-Control-Allow-Credentials");
    }

    [Fact]
    public async Task PreflightFromAllowedOrigin_ReceivesCorsHeaders()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "http://localhost:8081");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Single()
            .Should().Be("http://localhost:8081");
        response.Headers.Should().ContainKey("Access-Control-Allow-Credentials");
    }
}
