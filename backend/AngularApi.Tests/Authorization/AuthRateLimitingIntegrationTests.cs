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
    public async Task Login_ExceedingFiveRequestsPerMinute_ReturnsTooManyRequests()
    {
        var client = _factory.CreateClient();
        var payload = new LogInUserDTO { Email = "missing@example.com", Password = "WrongPassword123!" };

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/Account/login", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var limitedResponse = await client.PostAsJsonAsync("/api/Account/login", payload);
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
