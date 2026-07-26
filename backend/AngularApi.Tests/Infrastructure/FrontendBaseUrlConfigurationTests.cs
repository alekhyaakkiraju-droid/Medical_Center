using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class FrontendBaseUrlConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string BackendRoot = Path.Combine(RepoRoot, "backend", "AngularApi");

    [Fact]
    public void DockerComposeFile_ConfiguresFrontendBaseUrlFromEnvironment()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("Jwt__FrontendBaseUrl: ${FRONTEND_BASE_URL:-http://localhost:8081}");
    }

    [Fact]
    public void EnvExample_DocumentsFrontendBaseUrl()
    {
        var envExample = File.ReadAllText(Path.Combine(RepoRoot, ".env.example"));

        envExample.Should().Contain("FRONTEND_BASE_URL=http://localhost:8081");
    }

    [Fact]
    public void AppSettings_DefinesFrontendBaseUrlInJwtSection()
    {
        var appSettings = File.ReadAllText(Path.Combine(BackendRoot, "appsettings.json"));

        appSettings.Should().Contain("\"FrontendBaseUrl\"");
        appSettings.Should().NotContain("localhost:4200");
    }

    [Fact]
    public void AuthSourceFiles_HaveNoHardcodedLocalhost4200()
    {
        var authSourcePaths = new[]
        {
            Path.Combine(BackendRoot, "Controllers", "AccountController.cs"),
            Path.Combine(BackendRoot, "Options", "AuthCookieOptions.cs"),
        };

        foreach (var path in authSourcePaths)
        {
            File.ReadAllText(path).Should().NotContain("localhost:4200", because: $"{path} must use configurable FrontendBaseUrl");
        }
    }
}
