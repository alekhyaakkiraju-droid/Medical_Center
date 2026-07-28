using System.Text.RegularExpressions;
using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class EnvironmentConfigurationConsistencyTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void DockerCompose_EnvironmentVariables_DocumentedInEnvExample()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));
        var envExample = File.ReadAllText(Path.Combine(RepoRoot, ".env.example"));

        var variables = Regex.Matches(compose, @"\$\{([A-Z0-9_]+)(?::-(?:[^}]+))?\}")
            .Select(match => match.Groups[1].Value)
            .Distinct();

        foreach (var variable in variables)
        {
            envExample.Should().Contain(variable, because: $"{variable} should be documented in .env.example");
        }
    }

    [Fact]
    public void FrontendDockerfile_ApiBaseUrlDefault_MatchesDockerComposeDefault()
    {
        var dockerfile = File.ReadAllText(Path.Combine(RepoRoot, "front-end", "Dockerfile"));
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        var dockerfileMatch = Regex.Match(dockerfile, @"ARG\s+API_BASE_URL=(?<value>\S+)");
        var composeMatch = Regex.Match(compose, @"API_BASE_URL:\s*\$\{API_PUBLIC_URL:-(?<value>[^}]+)\}");

        dockerfileMatch.Success.Should().BeTrue();
        composeMatch.Success.Should().BeTrue();
        dockerfileMatch.Groups["value"].Value.Should().Be(composeMatch.Groups["value"].Value);
    }

    [Fact]
    public void EnvExample_FrontendBaseUrl_MatchesDockerComposeDefault()
    {
        var envExample = File.ReadAllText(Path.Combine(RepoRoot, ".env.example"));
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        envExample.Should().Contain("FRONTEND_BASE_URL=http://localhost:8081");
        compose.Should().Contain("Jwt__FrontendBaseUrl: ${FRONTEND_BASE_URL:-http://localhost:8081}");
    }
}
