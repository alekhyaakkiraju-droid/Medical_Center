using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class Angular22UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void PackageJson_PinsAngularPackagesTo22()
    {
        var packageJson = File.ReadAllText(Path.Combine(FrontendRoot, "package.json"));

        packageJson.Should().Contain("\"@angular/core\": \"^22.");
        packageJson.Should().Contain("\"@angular/cli\": \"^22.");
        packageJson.Should().Contain("\"@angular/material\": \"^22.");
        packageJson.Should().Contain("\"typescript\": \"~6.0.");
        packageJson.Should().NotContain("\"@angular/core\": \"^19.");
    }

    [Fact]
    public void ServerTs_UsesAngularSsrNodeEntryPoint()
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
    public void BoardTemplate_UsesBuiltInControlFlowSyntax()
    {
        var boardTemplate = File.ReadAllText(
            Path.Combine(FrontendRoot, "src", "app", "admin", "pages", "board", "board.component.html"));

        boardTemplate.Should().Contain("@if");
        boardTemplate.Should().Contain("@for");
    }
}
