using AngularApi.Options;
using FluentAssertions;

namespace AngularApi.Tests.Options;

public class AuthCookieOptionsTests
{
    [Fact]
    public void FrontendBaseUrl_TrimsTrailingSlashes()
    {
        var options = new AuthCookieOptions
        {
            FrontendBaseUrl = "https://staging.example.com/"
        };

        options.FrontendBaseUrl.Should().Be("https://staging.example.com");
    }

    [Fact]
    public void FrontendLoginSuccessUrl_DerivesFromFrontendBaseUrl()
    {
        var options = new AuthCookieOptions
        {
            FrontendBaseUrl = "https://staging.example.com"
        };

        options.FrontendLoginSuccessUrl.Should().Be("https://staging.example.com/auth/login-success");
    }

    [Fact]
    public void FrontendLoginSuccessUrl_UsesExplicitValueWhenConfigured()
    {
        var options = new AuthCookieOptions
        {
            FrontendBaseUrl = "https://staging.example.com",
            FrontendLoginSuccessUrl = "https://custom.example.com/success"
        };

        options.FrontendLoginSuccessUrl.Should().Be("https://custom.example.com/success");
    }
}
