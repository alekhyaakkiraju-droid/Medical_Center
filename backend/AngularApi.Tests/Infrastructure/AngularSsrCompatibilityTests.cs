using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class AngularSsrCompatibilityTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void ServerTs_RetainsCommonEngineForStandaloneBootstrapUntilRouteManifestMigration()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("WO-060");
        serverSource.Should().Contain("CommonEngine");
        serverSource.Should().Contain("allowedHosts");
        serverSource.Should().Contain("bootstrap from './src/main.server'");
        serverSource.Should().NotContain("new AngularNodeAppEngine");
    }

    [Fact]
    public void MainServerTs_PassesBootstrapContextForAngular22Ssr()
    {
        var mainServerSource = File.ReadAllText(Path.Combine(FrontendRoot, "src", "main.server.ts"));

        mainServerSource.Should().Contain("BootstrapContext");
        mainServerSource.Should().Contain("bootstrapApplication(AppComponent, config, context)");
    }

    [Fact]
    public void ReloadService_GuardsDomAccessDuringServerSideRendering()
    {
        var reloadService = File.ReadAllText(
            Path.Combine(FrontendRoot, "src", "app", "shared", "service", "reload.service.ts"));

        reloadService.Should().Contain("isPlatformBrowser");
    }

    [Fact]
    public void SsrSmokeTestScript_ValidatesAngular22PublicRoutesAndApiExclusion()
    {
        var script = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "ssr-smoke-tests.sh"));

        script.Should().Contain("WO-060");
        script.Should().Contain("/pages/about-us");
        script.Should().Contain("/pages/service");
        script.Should().Contain("/pages/gallery");
        script.Should().Contain("/pages/blog");
        script.Should().Contain("/pages/contact");
        script.Should().Contain("/pages/team");
        script.Should().Contain("/pages/appointment");
        script.Should().Contain("/api/nonexistent");
    }
}
