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
        yaml.Should().Contain("Expanded Smoke Tests");
        yaml.Should().Contain("Patient Journey E2E");
        yaml.Should().Contain("./scripts/run-e2e-smoke.sh");
        yaml.Should().Contain("./scripts/e2e-patient-journey.sh");
        yaml.Should().Contain("./scripts/smoke-tests.sh");
        yaml.Should().Contain("engine: opsera");
    }

    [Fact]
    public void ForgePipelineFile_IncludesRegistryPushAndProductionPromotion()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("CONTAINER_REGISTRY_URL");
        yaml.Should().Contain("PRODUCTION_BASE_URL");
        yaml.Should().Contain("variantId: push:ecr");
        yaml.Should().Contain("Push API Image to Registry");
        yaml.Should().Contain("Push YARP Image to Registry");
        yaml.Should().Contain("Push Frontend Image to Registry");
        yaml.Should().Contain("tag: \"$GIT_SHA\"");
        yaml.Should().Contain("tag: latest");
        yaml.Should().Contain("Expanded Smoke Tests");
        yaml.Should().Contain("Patient Journey E2E");
        yaml.Should().Contain("./scripts/run-e2e-smoke.sh");
        yaml.Should().Contain("./scripts/e2e-patient-journey.sh");
        yaml.Should().Contain("Production Promotion Gate");
        yaml.Should().Contain("variantId: gate:idp-approval");
        yaml.Should().Contain("Production Deploy API");
        yaml.Should().Contain("variantId: deploy:aws_ecs");
        yaml.Should().Contain("Production Post-Deploy Smoke Tests");
        yaml.Should().Contain("Production Rollback");
    }

    [Fact]
    public void ForgePipelineFile_IncludesDastScanStepAfterStagingE2eBeforeProductionGate()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("DAST Scan");
        yaml.Should().Contain("variantId: scan:zap");
        yaml.Should().Contain(".forge/zap-baseline.conf");
        yaml.Should().Contain("zap-dast-report.json");
        yaml.Should().Contain("zap-dast-report.xml");
        yaml.Should().Contain("timeoutMinutes: 10");
        yaml.Should().Contain("archiveArtifacts:");

        var dastIndex = yaml.IndexOf("DAST Scan", StringComparison.Ordinal);
        var patientJourneyIndex = yaml.IndexOf("Patient Journey E2E", StringComparison.Ordinal);
        var gateIndex = yaml.IndexOf("Production Promotion Gate", StringComparison.Ordinal);

        dastIndex.Should().BeGreaterThan(patientJourneyIndex, because: "DAST must run after patient journey E2E");
        dastIndex.Should().BeLessThan(gateIndex, because: "DAST must complete before production promotion");

        var stagingBlockStart = yaml.IndexOf("staging:", StringComparison.Ordinal);
        var productionBlockStart = yaml.IndexOf("production:", StringComparison.Ordinal);
        var stagingSteps = yaml[stagingBlockStart..productionBlockStart];
        stagingSteps.Should().Contain("- DAST Scan");
    }

    [Fact]
    public void ZapBaselineConfig_ExistsWithTargetUrlsAndExclusions()
    {
        var configPath = Path.Combine(RepoRoot, ".forge", "zap-baseline.conf");
        File.Exists(configPath).Should().BeTrue(because: "WO-006 requires a committed ZAP baseline configuration");

        var config = File.ReadAllText(configPath);
        config.Should().Contain("STAGING_BASE_URL");
        config.Should().Contain("STAGING_API_URL");
        config.Should().Contain("swagger/v1/swagger.json");
        config.Should().Contain("IGNORE");
        config.Should().Contain("#");
    }

    [Fact]
    public void ForgePipelineFile_DefinesProductionEnvironmentBlock()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("production:");
        yaml.Should().Contain("Production Promotion Gate");
        yaml.Should().Contain("Production Post-Deploy Smoke Tests");
        yaml.Should().Contain("Production Rollback");
    }

    [Fact]
    public void DeploymentRunbook_DocumentsRollbackWithinFiveMinutes()
    {
        var runbook = File.ReadAllText(Path.Combine(RepoRoot, "docs", "deployment-runbook.md"));

        runbook.Should().Contain("Rollback");
        runbook.Should().Contain("5 minutes");
        runbook.Should().Contain("$PREVIOUS_GIT_SHA");
    }

    [Fact]
    public void DeploymentRunbook_ContainsRequiredSectionHeadings()
    {
        var runbook = File.ReadAllText(Path.Combine(RepoRoot, "docs", "deployment-runbook.md"));

        runbook.Should().Contain("## Prerequisites");
        runbook.Should().Contain("## Pre-Deployment Checklist");
        runbook.Should().Contain("## Staging-to-Production Promotion");
        runbook.Should().Contain("## Rollback Procedure (5 min)");
        runbook.Should().Contain("## Environment Configuration");
        runbook.Should().Contain("## Database Migration Verification");
        runbook.Should().Contain("## Secrets Management (Docker Secrets from WO-003)");
        runbook.Should().Contain("## Health Check Validation");
        runbook.Should().Contain("## Troubleshooting");
    }

    [Fact]
    public void DeploymentRunbook_ReferencesPipelineAndOperationalFiles()
    {
        var runbook = File.ReadAllText(Path.Combine(RepoRoot, "docs", "deployment-runbook.md"));

        runbook.Should().Contain(".forge/pipeline.yaml");
        runbook.Should().Contain("docker-compose.yml");
        runbook.Should().Contain("scripts/smoke-tests.sh");
        runbook.Should().Contain("scripts/run-e2e-smoke.sh");
        runbook.Should().Contain("secrets.example/");
    }

    [Fact]
    public void DeploymentRunbook_IncludesDastInPrePromotionChecklist()
    {
        var runbook = File.ReadAllText(Path.Combine(RepoRoot, "docs", "deployment-runbook.md"));

        runbook.Should().Contain("DAST Scan");
        runbook.Should().Contain("zap-dast-report.json");
        runbook.Should().Contain(".forge/zap-baseline.conf");
    }
}
