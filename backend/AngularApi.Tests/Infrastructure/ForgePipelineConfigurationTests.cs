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
        yaml.Should().Contain("buildContext: backend");
        yaml.Should().Contain("build-api-docker.sh");
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

        yaml.Should().Contain("Post-Deploy Verification Gate");
        yaml.Should().Contain("Expanded Smoke Tests");
        yaml.Should().Contain("Patient Journey E2E");
        yaml.Should().Contain("./scripts/run-e2e-smoke.sh");
        yaml.Should().Contain("./scripts/e2e-patient-journey.sh");
        yaml.Should().Contain("post-deploy-gate.sh");
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

    [Fact]
    public void ForgePipelineFile_UsesDotNet10ForBackendBuild()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("DOTNET_VERSION:", because: "WO-049 requires a pipeline variable for the .NET SDK version");
        yaml.Should().Contain("value: \"10.0\"", because: "WO-049 requires DOTNET_VERSION to be 10.0");
        yaml.Should().Contain("dotnetVersion: \"10.0\"", because: "WO-049 requires the build:dotnet step to use .NET 10");
        yaml.Should().NotContain("value: \"8.0\"", because: "WO-049 must remove .NET 8 pipeline version references");
        yaml.Should().NotContain("dotnetVersion: \"8.0\"", because: "WO-049 must remove .NET 8 build step configuration");
    }

    [Fact]
    public void ForgePipelineFile_BackendBuildAndTestCommandsReferenceCsprojWithoutHardcodedSdk()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("buildCommand: dotnet build backend/AngularApi/AngularApi.csproj -c Release");
        yaml.Should().Contain("testCommand: dotnet test backend/AngularApi.Tests/AngularApi.Tests.csproj -c Release");
        yaml.Should().Contain("testCommand: dotnet test backend/AngularApi.Tests/AngularApi.Tests.csproj --filter FullyQualifiedName~Authorization -c Release");
        yaml.Should().NotContain("--framework net8.0", because: "backend build/test commands should rely on csproj target frameworks");
    }

    [Fact]
    public void ForgePipelineFile_IncludesAuthorizationRegressionGateInDevAndStaging()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("Authorization Regression Gate");
        yaml.Should().Contain("FullyQualifiedName~Authorization");

        var backendTestsIndex = yaml.IndexOf("- name: Backend Unit Tests", StringComparison.Ordinal);
        var authGateIndex = yaml.IndexOf("- name: Authorization Regression Gate", StringComparison.Ordinal);
        authGateIndex.Should().BeGreaterThan(backendTestsIndex);

        var devBlockStart = yaml.IndexOf("environments:", StringComparison.Ordinal);
        var stagingBlockStart = yaml.IndexOf("  staging:", StringComparison.Ordinal);
        var productionBlockStart = yaml.IndexOf("  production:", StringComparison.Ordinal);

        yaml[devBlockStart..stagingBlockStart].Should().Contain("- Authorization Regression Gate");
        yaml[stagingBlockStart..productionBlockStart].Should().Contain("- Authorization Regression Gate");
    }

    [Fact]
    public void ForgePipelineFile_FrontendBuildUsesNpmRunBuildWithOpenApiGeneration()
    {
        var yaml = File.ReadAllText(PipelinePath);
        yaml.Should().Contain("- name: Generate OpenAPI Spec");
        yaml.Should().Contain("testCommand: ./scripts/generate-openapi.sh");
        yaml.Should().Contain("npm run build -- --configuration production");
        yaml.Should().NotContain("./node_modules/.bin/ng build --configuration production");
    }

    [Fact]
    public void ForgePipelineFile_IncludesPlaceholderContentLintStep()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("Placeholder Content Lint");
        yaml.Should().Contain("check-placeholder-content.sh");

        var frontendTestsIndex = yaml.IndexOf("- name: Frontend Unit Tests", StringComparison.Ordinal);
        var lintIndex = yaml.IndexOf("- name: Placeholder Content Lint", StringComparison.Ordinal);
        lintIndex.Should().BeGreaterThan(frontendTestsIndex, because: "placeholder lint should run after frontend build/tests");

        var devBlockStart = yaml.IndexOf("environments:", StringComparison.Ordinal);
        var stagingBlockStart = yaml.IndexOf("  staging:", StringComparison.Ordinal);
        var productionBlockStart = yaml.IndexOf("  production:", StringComparison.Ordinal);

        yaml[devBlockStart..stagingBlockStart].Should().Contain("- Placeholder Content Lint");
        yaml[stagingBlockStart..productionBlockStart].Should().Contain("- Placeholder Content Lint");
    }

    [Fact]
    public void PlaceholderContentLintScript_ExistsAndIsExecutable()
    {
        var scriptPath = Path.Combine(RepoRoot, "scripts", "check-placeholder-content.sh");
        File.Exists(scriptPath).Should().BeTrue(because: "WO-020 requires a placeholder content lint script");

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("Lorem ipsum");
        script.Should().Contain("PrimeCare");
        script.Should().Contain("front-end/src/app");
        script.Should().Contain("exit 1");
    }
}
