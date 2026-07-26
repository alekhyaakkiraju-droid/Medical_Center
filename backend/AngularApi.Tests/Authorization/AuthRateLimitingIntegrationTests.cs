using System.Net;
using System.Net.Http.Json;
using AngularApi.DTO;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Authorization;

public class AuthRateLimitingIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public AuthRateLimitingIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithinFiveRequestsPerMinute_ReturnsNonRateLimitedStatus()
    {
        var client = CreateClientForIp("198.51.100.10");
        var payload = new LogInUserDTO { Email = "missing@example.com", Password = "WrongPassword123!" };

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/Account/login", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task Login_ExceedingFiveRequestsPerMinute_ReturnsTooManyRequests()
    {
        var client = CreateClientForIp("198.51.100.11");
        var payload = new LogInUserDTO { Email = "missing@example.com", Password = "WrongPassword123!" };

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/Account/login", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var limitedResponse = await client.PostAsJsonAsync("/api/Account/login", payload);
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Login_RateLimitIsPartitionedByClientIp()
    {
        var clientA = _factory.CreateClient();
        var clientB = _factory.CreateClient();
        var payload = new LogInUserDTO { Email = "missing@example.com", Password = "WrongPassword123!" };

        clientA.DefaultRequestHeaders.Add("X-Test-Client-Ip", "192.0.2.1");
        clientB.DefaultRequestHeaders.Add("X-Test-Client-Ip", "192.0.2.2");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await clientA.PostAsJsonAsync("/api/Account/login", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var limitedResponse = await clientA.PostAsJsonAsync("/api/Account/login", payload);
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var independentResponse = await clientB.PostAsJsonAsync("/api/Account/login", payload);
        independentResponse.StatusCode.Should().NotBe(
            HttpStatusCode.TooManyRequests,
            because: "each client IP maintains an independent fixed-window limiter partition");
    }

    private static HttpClient CreateClientForIp(MedicalCenterWebApplicationFactory factory, string ipAddress)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Client-Ip", ipAddress);
        return client;
    }

    private HttpClient CreateClientForIp(string ipAddress) => CreateClientForIp(_factory, ipAddress);
}
