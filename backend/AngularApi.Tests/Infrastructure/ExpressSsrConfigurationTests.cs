using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class ExpressSsrConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void ServerTs_HasApiExclusionAndGenericErrorHandler()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("app.all('/api/{*path}'");
        serverSource.Should().Contain("API endpoint not found");
        serverSource.Should().Contain("Internal Server Error");
        serverSource.Should().Contain("SSR rendering error:");
        serverSource.Should().Contain("res.headersSent");
    }

    [Fact]
    public void SsrSmokeTestScript_ValidatesRouteHardeningAcceptanceCriteria()
    {
        var script = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "ssr-smoke-tests.sh"));

        script.Should().Contain("WO-059");
        script.Should().Contain("/pages/about-us");
        script.Should().Contain("/pages/gallery");
        script.Should().Contain("/pages/team");
        script.Should().Contain("/api/nonexistent");
        script.Should().Contain("API endpoint not found");
    }
}
