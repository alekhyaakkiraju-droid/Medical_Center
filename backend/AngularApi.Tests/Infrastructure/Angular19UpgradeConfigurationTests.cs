using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class Angular19UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void PackageJson_PinsAngularPackagesTo19()
    {
        var packageJson = File.ReadAllText(Path.Combine(FrontendRoot, "package.json"));

        packageJson.Should().Contain("\"@angular/core\": \"^19.");
        packageJson.Should().Contain("\"@angular/cli\": \"^19.");
        packageJson.Should().Contain("\"@angular/material\": \"^19.");
        packageJson.Should().Contain("\"zone.js\": \"~0.15.");
        packageJson.Should().NotContain("\"@angular/core\": \"^18.");
    }

    [Fact]
    public void ServerTs_UsesAngular19SsrNodeEntryPoint()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("from '@angular/ssr/node'");
        serverSource.Should().NotContain("from '@angular/ssr';");
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

    [Fact]
    public void IndexHtml_DoesNotReferenceCdnAssets()
    {
        var indexHtml = File.ReadAllText(Path.Combine(FrontendRoot, "src", "index.html")).ToLowerInvariant();

        indexHtml.Should().NotContain("jquery");
        indexHtml.Should().NotContain("bootstrap");
        indexHtml.Should().NotContain("flowbite");
        indexHtml.Should().NotContain("<script");
    }
}
