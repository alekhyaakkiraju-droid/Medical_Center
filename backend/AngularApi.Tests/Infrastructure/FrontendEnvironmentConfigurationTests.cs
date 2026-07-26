using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class FrontendEnvironmentConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string FrontendSrcRoot = Path.Combine(RepoRoot, "front-end", "src");

    [Fact]
    public void ProductionEnvironment_UsesRelativeApiPath()
    {
        var environment = File.ReadAllText(Path.Combine(FrontendSrcRoot, "environments", "environment.ts"));

        environment.Should().Contain("production: true");
        environment.Should().Contain("api: \"/api\"");
        environment.Should().NotContain("localhost:5004");
    }

    [Fact]
    public void DevelopmentEnvironment_UsesLocalKestrelApiUrl()
    {
        var environment = File.ReadAllText(
            Path.Combine(FrontendSrcRoot, "environments", "environment.development.ts"));

        environment.Should().Contain("production: false");
        environment.Should().Contain("api: \"http://localhost:5004/api\"");
    }

    [Fact]
    public void FrontendSource_HasNoHardcodedLocalhost5004OutsideDevelopmentEnvironment()
    {
        var filesWithLocalhost = Directory
            .EnumerateFiles(FrontendSrcRoot, "*.*", SearchOption.AllDirectories)
            .Where(path =>
                (path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)
                 || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                && File.ReadAllText(path).Contains("localhost:5004", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(FrontendSrcRoot, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

        filesWithLocalhost.Should().BeEquivalentTo(["environments/environment.development.ts"]);
    }

    [Fact]
    public void AngularJson_DevelopmentConfiguration_ReplacesProductionEnvironmentFile()
    {
        var angularJson = File.ReadAllText(Path.Combine(RepoRoot, "front-end", "angular.json"));

        angularJson.Should().Contain("\"development\"");
        angularJson.Should().Contain("\"fileReplacements\"");
        angularJson.Should().Contain("\"replace\": \"src/environments/environment.ts\"");
        angularJson.Should().Contain("\"with\": \"src/environments/environment.development.ts\"");
    }

    [Fact]
    public void FrontendDockerfile_InjectsApiBaseUrlIntoProductionEnvironment()
    {
        var dockerfile = File.ReadAllText(Path.Combine(RepoRoot, "front-end", "Dockerfile"));

        dockerfile.Should().Contain("ARG API_BASE_URL=/api");
        dockerfile.Should().Contain("src/environments/environment.ts");
        dockerfile.Should().Contain("${API_BASE_URL}");
        dockerfile.Should().Contain("ng build --configuration production");
        dockerfile.Should().NotContain("localhost:5004");
    }

    [Fact]
    public void DockerComposeFile_DefaultsFrontendApiPublicUrlToRelativePath()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("API_BASE_URL: ${API_PUBLIC_URL:-/api}");
    }
}
