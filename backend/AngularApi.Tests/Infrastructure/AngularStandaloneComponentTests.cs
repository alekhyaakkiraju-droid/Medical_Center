using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class AngularStandaloneComponentTests
{
    private static readonly string FrontendAppRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "front-end", "src", "app"));

    [Fact]
    public void ProductionComponents_DoNotDeclareStandaloneFalse()
    {
        var componentFiles = Directory
            .EnumerateFiles(FrontendAppRoot, "*.component.ts", SearchOption.AllDirectories)
            .ToList();

        componentFiles.Should().NotBeEmpty();

        foreach (var file in componentFiles)
        {
            var source = File.ReadAllText(file);
            source.Should().NotContain("standalone: false", because: $"{file} should be standalone after WO-055");
            source.Should().Contain("imports:", because: $"{file} should declare direct imports after WO-055");
        }
    }

    [Fact]
    public void SharedModuleComponents_AreStandaloneAndImportable()
    {
        var sharedComponents = new[]
        {
            Path.Combine(FrontendAppRoot, "doctor", "pages", "delete-modal", "delete-modal.component.ts"),
            Path.Combine(FrontendAppRoot, "pages", "general", "Payment", "Payment.component.ts"),
            Path.Combine(FrontendAppRoot, "admin", "pages", "side-bar", "side-bar.component.ts"),
        };

        foreach (var file in sharedComponents)
        {
            File.Exists(file).Should().BeTrue(file);
            var source = File.ReadAllText(file);
            source.Should().NotContain("standalone: false", because: $"{file} must be standalone");
            source.Should().Contain("imports:", because: $"{file} must declare imports directly");
        }
    }
}
