using AngularApi.Options;
using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class GoogleAuthConfigurationTests
{
    [Theory]
    [InlineData(null, null, false)]
    [InlineData("", "", false)]
    [InlineData("   ", "   ", false)]
    [InlineData("placeholder", "placeholder", false)]
    [InlineData("real-client-id", "real-secret", true)]
    public void IsConfigured_ReturnsExpectedResult(string? clientId, string? clientSecret, bool expected)
    {
        var options = new GoogleAuthOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
        };

        options.IsConfigured.Should().Be(expected);
    }
}
