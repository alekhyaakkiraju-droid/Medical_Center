using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class ForgePipelineConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string PipelinePath = Path.Combine(RepoRoot, ".forge", "pipeline.yaml");

    [Fact]
    public void ForgePipelineFile_Exists()
    {
        File.Exists(PipelinePath).Should().BeTrue(because: "WO-024 requires a Forge Shipping pipeline configuration file");
    }

    [Fact]
    public void ForgePipelineFile_TriggersOnMainPushAndPullRequests()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("webhook_branch: main");
        yaml.Should().Contain("webhook_enabled: true");
        yaml.Should().Contain("branches: [main]");
        yaml.Should().Contain("pull_request:");
    }

    [Fact]
    public void ForgePipelineFile_IncludesBuildTestAndSecurityStages()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("variantId: build:dotnet");
        yaml.Should().Contain("variantId: build:node");
        yaml.Should().Contain("variantId: scan:gitleaks");
        yaml.Should().Contain("variantId: scan:semgrep");
        yaml.Should().Contain("variantId: test:generic");
        yaml.Should().Contain("npm audit");
    }

    [Fact]
    public void ForgePipelineFile_BuildsAllThreeDockerImages()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("dockerfile: backend/AngularApi/Dockerfile");
        yaml.Should().Contain("dockerfile: backend/YARPReverseProxy/Dockerfile");
        yaml.Should().Contain("dockerfile: front-end/Dockerfile");
        yaml.Should().Contain("variantId: build:docker");
        yaml.Should().Contain("variantId: scan:grype");
    }

    [Fact]
    public void ForgePipelineFile_FailsOnCriticalSecurityFindings()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("failOnBreach: fail");
        yaml.Should().Contain("severityThreshold: critical");
        yaml.Should().Contain("--audit-level=critical");
    }

    [Fact]
    public void ForgePipelineFile_DefinesStagingEnvironmentWithSmokeTests()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("Staging Smoke Tests");
        yaml.Should().Contain("./scripts/smoke-tests.sh");
        yaml.Should().Contain("engine: opsera");
    }
}
