using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

/// <summary>
/// Validates WO-031 staging IDP SMTP configuration artifacts.
/// </summary>
public class StagingEmailConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void DockerComposeStaging_OverridesApiSmtpSettingsWithIdpVariables()
    {
        var stagingCompose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.staging.yml"));

        stagingCompose.Should().Contain("SmtpSettings__Host: ${IDP_SMTP_HOST}");
        stagingCompose.Should().Contain("SmtpSettings__Port: ${IDP_SMTP_PORT:-587}");
        stagingCompose.Should().Contain("SmtpSettings__UseTls: ${IDP_SMTP_USE_TLS:-true}");
        stagingCompose.Should().Contain("EmailSettings__EmailUsername: ${IDP_SMTP_USERNAME}");
        stagingCompose.Should().Contain("EmailSettings__EmailPassword: ${IDP_SMTP_PASSWORD}");
        stagingCompose.Should().NotContain("IDP_SMTP_PASSWORD=actual");
    }

    [Fact]
    public void EnvStagingExample_DocumentsIdpSmtpVariablesWithoutHardcodedSecrets()
    {
        var envStaging = File.ReadAllText(Path.Combine(RepoRoot, ".env.staging.example"));

        envStaging.Should().Contain("IDP_SMTP_HOST=");
        envStaging.Should().Contain("IDP_SMTP_PORT=");
        envStaging.Should().Contain("IDP_SMTP_USE_TLS=");
        envStaging.Should().Contain("IDP_SMTP_USERNAME=");
        envStaging.Should().Contain("IDP_SMTP_PASSWORD=");
        envStaging.Should().Contain("IDP_SMTP_WEB_URL=");
        envStaging.Should().NotContain("password=secret");
    }

    [Fact]
    public void EnvExample_DocumentsStagingSmtpPatternAndPipelineSecrets()
    {
        var envExample = File.ReadAllText(Path.Combine(RepoRoot, ".env.example"));

        envExample.Should().Contain("WO-031");
        envExample.Should().Contain(".env.staging.example");
        envExample.Should().Contain("docker-compose.staging.yml");
        envExample.Should().Contain("docs/email-strategy.md");
        envExample.Should().Contain("Forge pipeline secrets");
    }

    [Fact]
    public void EmailStrategyDoc_DescribesDualMailHogAndIdpSmtpStrategy()
    {
        var doc = File.ReadAllText(Path.Combine(RepoRoot, "docs", "email-strategy.md"));

        doc.Should().Contain("## Overview");
        doc.Should().Contain("MailHog");
        doc.Should().Contain("localhost:8025");
        doc.Should().Contain("Forge IDP SMTP");
        doc.Should().Contain("Production");
        doc.Should().Contain("pipeline secrets");
    }

    [Fact]
    public void PipelineYaml_InjectsIdpSmtpSecretsOnStagingApiDeploy()
    {
        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));

        pipeline.Should().Contain("IDP_SMTP_HOST:");
        pipeline.Should().Contain("IDP_SMTP_USERNAME:");
        pipeline.Should().Contain("IDP_SMTP_PASSWORD:");
        pipeline.Should().Contain("SmtpSettings__Host: \"${IDP_SMTP_HOST}\"");
        pipeline.Should().Contain("EmailSettings__EmailUsername: \"${IDP_SMTP_USERNAME}\"");
        pipeline.Should().Contain("EmailSettings__EmailPassword: \"${IDP_SMTP_PASSWORD}\"");
    }

    [Fact]
    public void PipelineYaml_UsesHealthSmokeTestsForStagingSmtpVerification()
    {
        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));

        pipeline.Should().Contain("Post-Deploy Verification Gate");
        pipeline.Should().Contain("post-deploy-gate.sh");
        pipeline.Should().Contain("run-e2e-smoke.sh");
    }
}
