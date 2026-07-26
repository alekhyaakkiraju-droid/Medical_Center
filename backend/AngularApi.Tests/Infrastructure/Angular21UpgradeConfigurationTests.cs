using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class Angular21UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void PackageJson_PinsAngularPackagesTo21()
    {
        var packageJson = File.ReadAllText(Path.Combine(FrontendRoot, "package.json"));

        packageJson.Should().Contain("\"@angular/core\": \"^21.");
        packageJson.Should().Contain("\"@angular/cli\": \"^21.");
        packageJson.Should().Contain("\"@angular/material\": \"^21.");
        packageJson.Should().Contain("\"typescript\": \"~5.8.");
        packageJson.Should().Contain("\"ngx-toastr\": \"^20.");
        packageJson.Should().NotContain("\"@angular/core\": \"^20.");
    }

    [Fact]
    public void ServerTs_UsesSsrNodeEntryPointForEsmRuntime()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));
        serverSource.Should().Contain("from '@angular/ssr/node'");
    }

    [Fact]
    public void AngularJson_PreservesNgModuleSchematicsWithStandaloneFalse()
    {
        var angularJson = File.ReadAllText(Path.Combine(FrontendRoot, "angular.json"));
        angularJson.Should().Contain("\"standalone\": false");
    }
}
