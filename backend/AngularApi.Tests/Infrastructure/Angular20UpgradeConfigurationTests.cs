using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class Angular20UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void PackageJson_MeetsAngular20NodeAndVersionRequirements()
    {
        var packageJson = File.ReadAllText(Path.Combine(FrontendRoot, "package.json"));

        packageJson.Should().Contain("\"node\": \">=20\"");
        packageJson.Should().NotContain("\"@angular/core\": \"^19.");
        packageJson.Should().NotContain("\"@angular/core\": \"^18.");
        packageJson.Should().MatchRegex("\"@angular/core\": \"\\^2[0-9]");
    }

    [Fact]
    public void ServerTs_UsesSsrNodeEntryPointForEsmRuntime()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("from '@angular/ssr/node'");
        serverSource.Should().Contain("CommonEngine");
    }

    [Fact]
    public void AngularJson_DefaultsComponentSchematicsToStandaloneTrue()
    {
        var angularJson = File.ReadAllText(Path.Combine(FrontendRoot, "angular.json"));

        angularJson.Should().Contain("\"standalone\": true");
    }

    [Fact]
    public void SampleComponent_UsesStandaloneImportsArray()
    {
        var appComponent = File.ReadAllText(Path.Combine(FrontendRoot, "src", "app", "app.component.ts"));

        appComponent.Should().Contain("imports:");
    }
}
