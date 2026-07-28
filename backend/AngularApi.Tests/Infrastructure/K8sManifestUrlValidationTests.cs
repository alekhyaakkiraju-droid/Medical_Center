using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class K8sManifestUrlValidationTests
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
            .Select(entry => $"{Path.GetRelativePath(RepoRoot, entry.file)}:{entry.lineNumber}: {entry.line.Trim()}")
            .ToList();

        violations.Should().BeEmpty("K8s manifests must not contain localhost references: {0}", string.Join("; ", violations));
    }

    [Fact]
    public void ApiDeployment_FrontendBaseUrl_MatchesDevEksPattern()
    {
        var deployment = File.ReadAllText(Path.Combine(K8sRoot, "api-deployment.yaml"));

        deployment.Should().Contain("Jwt__FrontendBaseUrl");
        deployment.Should().MatchRegex(@"https://[a-z0-9-]+\.agent\.opsera\.dev");
    }
}
