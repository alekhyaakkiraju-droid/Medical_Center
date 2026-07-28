using System.Text.RegularExpressions;
using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class K8sManifestConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string K8sRoot = Path.Combine(RepoRoot, ".opsera-medical-center", "k8s");

    [Fact]
    public void K8sManifests_ContainNoLocalhostReferences()
    {
        var violations = Directory
            .EnumerateFiles(K8sRoot, "*.yaml", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadLines(file).Select((line, index) => (file, lineNumber: index + 1, line)))
            .Where(entry => entry.line.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            .Select(entry => $"{Path.GetRelativePath(RepoRoot, entry.file)}:{entry.lineNumber}")
            .ToList();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void ApiDeployment_FrontendBaseUrl_MatchesDevEksPattern()
    {
        var deployment = File.ReadAllText(Path.Combine(K8sRoot, "api-deployment.yaml"));
        var match = Regex.Match(deployment, @"Jwt__FrontendBaseUrl\s*\n\s*value:\s*(?<url>\S+)");

        match.Success.Should().BeTrue();
        match.Groups["url"].Value.Should().MatchRegex(@"https://[a-z0-9-]+\.agent\.opsera\.dev");
    }

    [Fact]
    public void ApiDeployment_CorsOrigin_MatchesFrontendBaseUrl()
    {
        var deployment = File.ReadAllText(Path.Combine(K8sRoot, "api-deployment.yaml"));

        var frontendMatch = Regex.Match(deployment, @"Jwt__FrontendBaseUrl\s*\n\s*value:\s*(?<url>\S+)");
        var corsMatch = Regex.Match(deployment, @"CorsSettings__AllowedOrigins__0\s*\n\s*value:\s*(?<url>\S+)");

        frontendMatch.Success.Should().BeTrue();
        corsMatch.Success.Should().BeTrue();
        corsMatch.Groups["url"].Value.Should().Be(frontendMatch.Groups["url"].Value);
    }

    [Fact]
    public void ApiDeployment_SmtpHost_IsMailhogForDev()
    {
        var deployment = File.ReadAllText(Path.Combine(K8sRoot, "api-deployment.yaml"));

        deployment.Should().Contain("SmtpSettings__Host");
        deployment.Should().Contain("medical-center-mailhog");
    }
}
