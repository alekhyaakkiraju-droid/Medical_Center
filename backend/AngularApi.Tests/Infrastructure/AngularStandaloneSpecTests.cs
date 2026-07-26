using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class AngularStandaloneSpecTests
{
    private static readonly string FrontendAppRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "front-end", "src", "app"));

    private static readonly string FrontendSrcRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "front-end", "src"));

    private static readonly string[] DeletedNgModuleNames =
    [
        "SharedModule",
        "AdminModule",
        "DoctorModule",
        "GeneralModule",
        "AuthModule",
        "AppModule",
    ];

    [Fact]
    public void ComponentSpecFiles_DoNotReferenceDeletedNgModules()
    {
        var specFiles = Directory
            .EnumerateFiles(FrontendAppRoot, "*.spec.ts", SearchOption.AllDirectories)
            .ToList();

        specFiles.Should().HaveCountGreaterThanOrEqualTo(28);

        foreach (var file in specFiles)
        {
            var source = File.ReadAllText(file);
            foreach (var moduleName in DeletedNgModuleNames)
            {
                source.Should().NotContain(moduleName, because: $"{file} must not import deleted NgModules after WO-057");
            }
        }
    }

    [Fact]
    public void ComponentSpecFiles_DoNotUseDeclarationsArrays()
    {
        var specFiles = Directory
            .EnumerateFiles(FrontendAppRoot, "*.spec.ts", SearchOption.AllDirectories)
            .ToList();

        foreach (var file in specFiles)
        {
            var source = File.ReadAllText(file);
            source.Should().NotMatchRegex(
                @"declarations\s*:\s*\[",
                because: $"{file} must import standalone components instead of declaring them");
        }
    }

    [Fact]
    public void AccessibilitySpec_UsesStandaloneStubComponents()
    {
        var accessibilitySpec = File.ReadAllText(Path.Combine(FrontendAppRoot, "accessibility", "accessibility.spec.ts"));

        accessibilitySpec.Should().Contain("standalone: true", because: "accessibility stub components must be standalone");
        accessibilitySpec.Split("standalone: true", StringSplitOptions.None).Length.Should().Be(5);
    }

    [Fact]
    public void SharedModuleSpec_IsReplacedWithStandaloneComponentSpec()
    {
        File.Exists(Path.Combine(FrontendAppRoot, "shared", "shared.module.spec.ts"))
            .Should()
            .BeFalse(because: "SharedModule spec must be removed after WO-057");

        var replacement = File.ReadAllText(Path.Combine(FrontendAppRoot, "shared", "shared-standalone-components.spec.ts"));
        replacement.Should().Contain("imports: [DeleteModalComponent, PaymentComponent, SideBarComponent]");
        replacement.Should().NotContain("SharedModule");
    }

    [Fact]
    public void FrontendContainsStandaloneSpecMigrationGate()
    {
        var gate = File.ReadAllText(Path.Combine(FrontendSrcRoot, "angular-standalone-spec-migration.spec.ts"));
        gate.Should().Contain("WO-057");
        gate.Should().Contain("declarations:");
        gate.Should().Contain("SharedModule");
    }
}
