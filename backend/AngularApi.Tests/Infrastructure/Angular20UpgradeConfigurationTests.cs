using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class Angular20UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void PackageJson_PinsAngularPackagesTo20()
    {
        var packageJson = File.ReadAllText(Path.Combine(FrontendRoot, "package.json"));

        packageJson.Should().Contain("\"@angular/core\": \"^20.");
        packageJson.Should().Contain("\"@angular/cli\": \"^20.");
        packageJson.Should().Contain("\"@angular/material\": \"^20.");
        packageJson.Should().Contain("\"typescript\": \"~5.8.");
        packageJson.Should().Contain("\"node\": \">=20\"");
        packageJson.Should().NotContain("\"@angular/core\": \"^19.");
    }

    [Fact]
    public void ServerTs_UsesSsrNodeEntryPointForEsmRuntime()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("from '@angular/ssr/node'");
        serverSource.Should().Contain("CommonEngine");
    }

    [Fact]
    public void AngularJson_PreservesNgModuleSchematicsWithStandaloneFalse()
    {
        var angularJson = File.ReadAllText(Path.Combine(FrontendRoot, "angular.json"));

        angularJson.Should().Contain("\"standalone\": false");
    }

    [Fact]
    public void SampleComponent_DeclaresStandaloneFalseForNgModuleCompatibility()
    {
        var appComponent = File.ReadAllText(Path.Combine(FrontendRoot, "src", "app", "app.component.ts"));

        appComponent.Should().Contain("standalone: false");
    }
}
