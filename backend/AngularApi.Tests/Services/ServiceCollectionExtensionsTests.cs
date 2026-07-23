using AngularApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Services;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddAuthenticationServices_ConfiguresCorsWithAllowedOriginsFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:ValidIssuer"] = "test-issuer",
                ["Jwt:ValidAudience"] = "test-audience",
                ["Jwt:Secret"] = "ThisIsAVeryLongSecretKeyForTestingPurposes123!",
                ["Jwt:AuthCookieName"] = "MedCenter.Auth",
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
                ["Cors:AllowedOrigins:1"] = "https://app.example.com",
                ["GoogleAuth:ClientId"] = "test-client-id",
                ["GoogleAuth:ClientSecret"] = "test-client-secret",
            })
            .Build();

        configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            .Should()
            .BeEquivalentTo(new[] { "http://localhost:4200", "https://app.example.com" });

        var services = new ServiceCollection();
        services.AddAuthenticationServices(configuration);
        await using var provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<ICorsPolicyProvider>();
        var policy = await policyProvider.GetPolicyAsync(new DefaultHttpContext(), "MyPolicy");

        policy.Should().NotBeNull();
        policy!.Origins.Should().BeEquivalentTo(new[]
        {
            "http://localhost:4200",
            "https://app.example.com",
        });
        policy.SupportsCredentials.Should().BeTrue();
    }
}
