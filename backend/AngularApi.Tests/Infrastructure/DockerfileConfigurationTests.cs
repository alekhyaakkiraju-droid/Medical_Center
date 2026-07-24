using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class DockerfileConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Theory]
    [InlineData("front-end/Dockerfile", "node:22-alpine", "HEALTHCHECK", "USER nginx", "ng build --configuration production", "npm ci --ignore-scripts")]
    [InlineData("backend/AngularApi/Dockerfile", "mcr.microsoft.com/dotnet/aspnet:8.0", "HEALTHCHECK", "USER $APP_UID", "/health")]
    [InlineData("backend/YARPReverseProxy/Dockerfile", "mcr.microsoft.com/dotnet/aspnet:8.0", "HEALTHCHECK", "USER $APP_UID", "${BUILD_CONFIGURATION}")]
    public void Dockerfiles_ContainProductionRequirements(string relativePath, params string[] requiredSnippets)
    {
        var dockerfile = File.ReadAllText(Path.Combine(RepoRoot, relativePath));

        foreach (var snippet in requiredSnippets)
        {
            dockerfile.Should().Contain(snippet, because: $"{relativePath} must include {snippet}");
        }
    }

    [Fact]
    public void YarpDockerfile_DoesNotUseWindowsNanoServerImages()
    {
        var dockerfile = File.ReadAllText(Path.Combine(RepoRoot, "backend/YARPReverseProxy/Dockerfile"));

        dockerfile.Should().NotContain("nanoserver");
        dockerfile.Should().NotContain("%BUILD_CONFIGURATION%");
    }

    [Fact]
    public void FrontendDockerignore_ExcludesBuildArtifacts()
    {
        var dockerignore = File.ReadAllText(Path.Combine(RepoRoot, "front-end/.dockerignore"));

        dockerignore.Should().Contain("node_modules");
        dockerignore.Should().Contain(".git");
        dockerignore.Should().Contain("dist");
        dockerignore.Should().Contain(".angular");
    }

    [Fact]
    public void NginxConfig_IncludesSecurityHeadersAndGzip()
    {
        var nginxConfig = File.ReadAllText(Path.Combine(RepoRoot, "front-end/nginx.conf"));

        nginxConfig.Should().Contain("Content-Security-Policy");
        nginxConfig.Should().Contain("Strict-Transport-Security");
        nginxConfig.Should().Contain("X-Frame-Options");
        nginxConfig.Should().Contain("X-Content-Type-Options");
        nginxConfig.Should().Contain("gzip on");
    }
}
