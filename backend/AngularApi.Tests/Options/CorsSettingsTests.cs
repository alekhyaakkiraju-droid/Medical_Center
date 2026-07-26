using AngularApi.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AngularApi.Tests.Configuration;

public class CorsPolicyConfigurationTests
{
    [Fact]
    public void BindFromConfiguration_ReadsAllowedOriginsArray()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:AllowedOrigins:0"] = "https://app.example.com",
                ["CorsSettings:AllowedOrigins:1"] = "https://staging.example.com"
            })
            .Build();

        var settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();

        settings.Should().NotBeNull();
        settings!.AllowedOrigins.Should().Equal(
            "https://app.example.com",
            "https://staging.example.com");
    }

    [Fact]
    public void DefaultValues_StartEmptyUntilConfigured()
    {
        var settings = new CorsSettings();

        settings.AllowedOrigins.Should().BeEmpty();
        CorsSettings.DefaultOrigins.Should().Contain("http://localhost:4200");
        CorsSettings.DefaultOrigins.Should().Contain("http://localhost:8081");
    }
}
