using AngularApi.Options;
using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class SmtpConfigurationTests
{
    [Fact]
    public void IsDevMode_ReturnsTrue_ForMailhogHost()
    {
        var settings = new SmtpSettings { Host = "medical-center-mailhog", Port = 1025 };

        settings.IsDevMode.Should().BeTrue();
    }

    [Fact]
    public void IsDevMode_ReturnsFalse_ForProductionHost()
    {
        var settings = new SmtpSettings { Host = "smtp.gmail.com", Port = 587 };

        settings.IsDevMode.Should().BeFalse();
    }

    [Fact]
    public void IsConfigured_ReturnsFalse_ForEmptyHost()
    {
        var settings = new SmtpSettings { Host = "", Port = 587 };

        settings.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void IsConfigured_ReturnsTrue_ForValidHostAndPort()
    {
        var settings = new SmtpSettings { Host = "smtp.example.com", Port = 587 };

        settings.IsConfigured.Should().BeTrue();
    }
}
