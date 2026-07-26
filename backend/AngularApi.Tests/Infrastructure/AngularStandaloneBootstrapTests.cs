using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class AngularStandaloneBootstrapTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");
    private static readonly string FrontendAppRoot = Path.Combine(FrontendRoot, "src", "app");

    [Fact]
    public void MainTs_BootstrapsWithBootstrapApplication()
    {
        var mainSource = File.ReadAllText(Path.Combine(FrontendRoot, "src", "main.ts"));
        mainSource.Should().Contain("bootstrapApplication");
        mainSource.Should().Contain("app.config");
        mainSource.Should().NotContain("bootstrapModule");
        mainSource.Should().NotContain("AppModule");
    }

    [Fact]
    public void AppConfig_ProvidesRouterAndHttpClient()
    {
        var configSource = File.ReadAllText(Path.Combine(FrontendAppRoot, "app.config.ts"));
        configSource.Should().Contain("export const appConfig");
        configSource.Should().Contain("provideRouter(routes)");
        configSource.Should().Contain("provideHttpClient");
    }

    [Fact]
    public void AppRoutes_UseStandaloneRouteFilesInsteadOfNgModules()
    {
        var routesSource = File.ReadAllText(Path.Combine(FrontendAppRoot, "app.routes.ts"));
        routesSource.Should().Contain("admin.routes");
        routesSource.Should().Contain("doctor.routes");
        routesSource.Should().Contain("general.routes");
        routesSource.Should().Contain("auth.routes");
        routesSource.Should().NotContain(".module");
    }

    [Fact]
    public void LegacyNgModuleBootstrapFiles_AreRemoved()
    {
        File.Exists(Path.Combine(FrontendAppRoot, "app.module.ts")).Should().BeFalse();
        File.Exists(Path.Combine(FrontendAppRoot, "app-routing.module.ts")).Should().BeFalse();
        File.Exists(Path.Combine(FrontendAppRoot, "app.module.server.ts")).Should().BeFalse();
        File.Exists(Path.Combine(FrontendAppRoot, "admin", "admin.module.ts")).Should().BeFalse();
        File.Exists(Path.Combine(FrontendAppRoot, "doctor", "doctor.module.ts")).Should().BeFalse();
        File.Exists(Path.Combine(FrontendAppRoot, "pages", "general", "general.module.ts")).Should().BeFalse();
        File.Exists(Path.Combine(FrontendAppRoot, "pages", "auth", "auth.module.ts")).Should().BeFalse();
        File.Exists(Path.Combine(FrontendAppRoot, "shared", "shared.module.ts")).Should().BeFalse();
    }

    [Fact]
    public void MainServerTs_ExportsStandaloneBootstrapFunction()
    {
        var mainServerSource = File.ReadAllText(Path.Combine(FrontendRoot, "src", "main.server.ts"));
        mainServerSource.Should().Contain("bootstrapApplication");
        mainServerSource.Should().Contain("app.config.server");
        mainServerSource.Should().NotContain("AppServerModule");
    }

    [Fact]
    public void ServerTs_UsesAngularNodeAppEngineWithoutLegacyAppServerModule()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));
        serverSource.Should().Contain("AngularNodeAppEngine");
        serverSource.Should().NotContain("AppServerModule");
        serverSource.Should().NotContain("CommonEngine");
    }

    [Fact]
    public void AppConfig_ConfiguresClientHydrationForSsr()
    {
        var configSource = File.ReadAllText(Path.Combine(FrontendAppRoot, "app.config.ts"));
        configSource.Should().Contain("provideClientHydration");
    }
}
