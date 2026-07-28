using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class K8sManifestProbeTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void ApiDeployment_IncludesStartupReadinessAndLivenessProbes()
    {
        var manifest = File.ReadAllText(Path.Combine(RepoRoot, ".opsera-medical-center", "k8s", "api-deployment.yaml"));
        manifest.Should().Contain("startupProbe:");
        manifest.Should().Contain("path: /health/ready");
        manifest.Should().Contain("path: /health");
        manifest.Should().Contain("failureThreshold: 30");
    }

    [Fact]
    public void FrontendDeployment_IncludesStartupProbe()
    {
        var manifest = File.ReadAllText(Path.Combine(RepoRoot, ".opsera-medical-center", "k8s", "frontend-deployment.yaml"));
        manifest.Should().Contain("startupProbe:");
        manifest.Should().Contain("path: /");
    }

    [Fact]
    public void YarpDeployment_IncludesStartupAndReadinessProbes()
    {
        var manifest = File.ReadAllText(Path.Combine(RepoRoot, ".opsera-medical-center", "k8s", "yarp-deployment.yaml"));
        manifest.Should().Contain("startupProbe:");
        manifest.Should().Contain("readinessProbe:");
        manifest.Should().Contain("path: /health");
    }
}
