using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class Angular21UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void PackageJson_PinsAngularPackagesTo21OrNewer()
    {
        var packageJson = File.ReadAllText(Path.Combine(FrontendRoot, "package.json"));

        packageJson.Should().MatchRegex("\"@angular/core\": \"\\^2[1-9]\\.");
        packageJson.Should().MatchRegex("\"@angular/cli\": \"\\^2[1-9]\\.");
        packageJson.Should().MatchRegex("\"@angular/material\": \"\\^2[1-9]\\.");
        packageJson.Should().NotContain("\"@angular/core\": \"^19.");
        packageJson.Should().NotContain("\"@angular/core\": \"^20.");
    }

    [Fact]
    public void ServerTs_UsesSsrNodeEntryPointForEsmRuntime()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));
        serverSource.Should().Contain("from '@angular/ssr/node'");
    }

    [Fact]
    public void AngularJson_DefaultsComponentSchematicsToStandaloneTrue()
    {
        var angularJson = File.ReadAllText(Path.Combine(FrontendRoot, "angular.json"));
        angularJson.Should().Contain("\"standalone\": true");
    }
}
