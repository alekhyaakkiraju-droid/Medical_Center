using AngularApi.Logging;
using FluentAssertions;

namespace AngularApi.Tests.Logging;

public class PiiMaskingTests
{
    [Fact]
    public void MaskEmail_RedactsLocalPart()
    {
        PiiMasking.MaskEmail("patient@example.com").Should().Be("pa***@example.com");
    }

    [Fact]
    public void MaskName_RedactsAfterFirstCharacter()
    {
        PiiMasking.MaskName("Jane Doe").Should().Be("J***");
    }
}
