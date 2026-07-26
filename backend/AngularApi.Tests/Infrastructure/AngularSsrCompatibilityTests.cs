using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class AngularSsrCompatibilityTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void ServerTs_UsesAngularNodeAppEngineInsteadOfDeprecatedCommonEngine()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("AngularNodeAppEngine");
        serverSource.Should().Contain("writeResponseToNodeResponse");
        serverSource.Should().Contain("createNodeRequestHandler");
        serverSource.Should().Contain("setAngularAppEngineManifest");
        serverSource.Should().NotContain("new CommonEngine");
        serverSource.Should().NotContain("import { CommonEngine");
    }

    [Fact]
    public void ServerTs_ExcludesApiPathsFromSsrRendering()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("/api/{*path}");
        serverSource.Should().Contain("API endpoint not found");
    }

    [Fact]
    public void ServerTs_IncludesExpressErrorHandlingMiddleware()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("Internal Server Error");
        serverSource.Should().Contain("headersSent");
        serverSource.Should().Contain("SSR rendering error");
    }

    [Fact]
    public void MainServerTs_ExportsServerBootstrapEntryPoint()
    {
        var mainServerSource = File.ReadAllText(
            Path.Combine(FrontendRoot, "src", "main.server.ts"));

        mainServerSource.Should().Contain("AppServerModule");
    }

    [Fact]
    public void AngularJson_EnablesSsrWithServerEntryPoint()
    {
        var angularJson = File.ReadAllText(Path.Combine(FrontendRoot, "angular.json"));

        angularJson.Should().Contain("\"server\": \"src/main.server.ts\"");
        angularJson.Should().Contain("\"entry\": \"server.ts\"");
        angularJson.Should().Contain("\"prerender\": false");
    }

    [Fact]
    public void AppModule_ConfiguresClientHydrationForSsr()
    {
        var appModuleSource = File.ReadAllText(
            Path.Combine(FrontendRoot, "src", "app", "app.module.ts"));

        appModuleSource.Should().Contain("provideClientHydration");
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
