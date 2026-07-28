using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class DockerfileConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Theory]
    [InlineData("front-end/Dockerfile", "node:22-alpine", "HEALTHCHECK", "nginx:1-alpine-slim", "ng build --configuration production", "npm ci --ignore-scripts")]
    [InlineData("backend/AngularApi/Dockerfile", "mcr.microsoft.com/dotnet/sdk:10.0", "mcr.microsoft.com/dotnet/aspnet:10.0", "HEALTHCHECK", "USER $APP_UID", "/health", "apt-get install -y --no-install-recommends curl")]
    [InlineData("backend/YARPReverseProxy/Dockerfile", "mcr.microsoft.com/dotnet/sdk:10.0", "mcr.microsoft.com/dotnet/aspnet:10.0", "HEALTHCHECK", "USER $APP_UID", "${BUILD_CONFIGURATION}", "apt-get install -y --no-install-recommends curl")]
    public void Dockerfiles_ContainProductionRequirements(string relativePath, params string[] requiredSnippets)
    {
        var dockerfile = File.ReadAllText(Path.Combine(RepoRoot, relativePath));

        foreach (var snippet in requiredSnippets)
        {
            dockerfile.Should().Contain(snippet, because: $"{relativePath} must include {snippet}");
        }
    }

    [Fact]
    public void ApiDockerfile_RequiresBackendDirectoryAsBuildContext()
    {
        var dockerfile = File.ReadAllText(Path.Combine(RepoRoot, "backend/AngularApi/Dockerfile"));
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        dockerfile.Should().Contain("AngularApi.Contracts/", because: "API Dockerfile copies the contracts project sibling");
        compose.Should().Contain("context: ./backend");
        compose.Should().Contain("dockerfile: AngularApi/Dockerfile");

        File.Exists(Path.Combine(RepoRoot, "scripts/build-api-docker.sh")).Should().BeTrue();
        var buildScript = File.ReadAllText(Path.Combine(RepoRoot, "scripts/build-api-docker.sh"));
        buildScript.Should().Contain("backend/AngularApi/Dockerfile");
        buildScript.Should().Contain(" backend");
        buildScript.Should().NotContain("backend/AngularApi\"");
    }

    [Fact]
    public void BackendDockerfiles_UseDotNet10UbuntuBaseImages()
    {
        foreach (var relativePath in new[] { "backend/AngularApi/Dockerfile", "backend/YARPReverseProxy/Dockerfile" })
        {
            var dockerfile = File.ReadAllText(Path.Combine(RepoRoot, relativePath));

            dockerfile.Should().NotContain(":8.0", because: $"{relativePath} must not reference .NET 8 base images");
            dockerfile.Should().Contain("mcr.microsoft.com/dotnet/sdk:10.0", because: $"{relativePath} build stage must use .NET 10 SDK");
            dockerfile.Should().Contain("mcr.microsoft.com/dotnet/aspnet:10.0", because: $"{relativePath} runtime stage must use .NET 10 ASP.NET");
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
        nginxConfig.Should().Contain("X-XSS-Protection");
        nginxConfig.Should().Contain("Referrer-Policy");
        nginxConfig.Should().Contain("gzip on");
        nginxConfig.Should().Contain("image/svg+xml");
    }

    [Fact]
    public void NginxConfig_IncludesSpaFallbackAndAssetCaching()
    {
        var nginxConfig = File.ReadAllText(Path.Combine(RepoRoot, "front-end/nginx.conf"));

        nginxConfig.Should().Contain("try_files $uri $uri/ /index.html");
        nginxConfig.Should().Contain("proxy_pass http://yarp-proxy:8080/api/");
        nginxConfig.Should().Contain("Cache-Control \"public, max-age=31536000, immutable\"");
        nginxConfig.Should().Contain("location = /index.html");
        nginxConfig.Should().Contain("Cache-Control \"no-cache\"");
    }
}
