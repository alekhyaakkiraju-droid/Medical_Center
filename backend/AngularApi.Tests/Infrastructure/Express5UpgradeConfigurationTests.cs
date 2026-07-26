using FluentAssertions;
using System.Text.Json;

namespace AngularApi.Tests.Infrastructure;

public class Express5UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendRoot = Path.Combine(RepoRoot, "front-end");

    [Fact]
    public void PackageJson_PinsExpressTo5()
    {
        var packageJson = File.ReadAllText(Path.Combine(FrontendRoot, "package.json"));

        packageJson.Should().Contain("\"express\": \"^5.");
        packageJson.Should().Contain("\"@types/express\": \"^5.");
        packageJson.Should().NotContain("\"express\": \"^4.");
    }

    [Fact]
    public void PackageLock_ResolvesExpress5WithoutExpress4DirectDependency()
    {
        var packageLockPath = Path.Combine(FrontendRoot, "package-lock.json");
        File.Exists(packageLockPath).Should().BeTrue();

        using var document = JsonDocument.Parse(File.ReadAllText(packageLockPath));
        var packages = document.RootElement.GetProperty("packages");

        packages.GetProperty("node_modules/express")
            .GetProperty("version")
            .GetString()
            .Should()
            .StartWith("5.");
    }

    [Fact]
    public void ServerTs_UsesExpress5CatchAllRouteSyntax()
    {
        var serverSource = File.ReadAllText(Path.Combine(FrontendRoot, "server.ts"));

        serverSource.Should().Contain("server.get('/{*path}'");
        serverSource.Should().NotContain("server.get('**'");
    }

    [Fact]
    public void SsrSmokeTestScript_ExistsAndValidatesExpress5Upgrade()
    {
        var scriptPath = Path.Combine(RepoRoot, "scripts", "ssr-smoke-tests.sh");
        File.Exists(scriptPath).Should().BeTrue();

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("WO-058");
        script.Should().Contain("serve:ssr:MedicalCenter");
        script.Should().Contain("app-root");
        script.Should().Contain("Cache-Control");
    }
}
