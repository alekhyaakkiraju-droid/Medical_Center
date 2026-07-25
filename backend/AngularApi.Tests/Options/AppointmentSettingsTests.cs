using AngularApi.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AngularApi.Tests.Configuration;

public class AppointmentSettingsTests
{
    [Fact]
    public void BindFromConfiguration_ReadsDefaultFeeAndCenterId()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppointmentSettings:DefaultFee"] = "45",
                ["AppointmentSettings:DefaultCenterId"] = "5"
            })
            .Build();

        var settings = configuration.GetSection(AppointmentSettings.SectionName).Get<AppointmentSettings>();

        settings.Should().NotBeNull();
        settings!.DefaultFee.Should().Be(45);
        settings.DefaultCenterId.Should().Be(5);
    }

    [Fact]
    public void DefaultValues_MatchLegacyHardcodedValues()
    {
        var settings = new AppointmentSettings();

        settings.DefaultFee.Should().Be(30);
        settings.DefaultCenterId.Should().Be(2);
    }
}
