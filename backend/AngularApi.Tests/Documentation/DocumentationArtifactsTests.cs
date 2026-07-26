using FluentAssertions;

namespace AngularApi.Tests.Documentation;

public class DocumentationArtifactsTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    public static IEnumerable<object[]> RequiredDocumentationFiles =>
        new List<object[]>
        {
            new object[] { "docs/adr/001-modular-monolith.md", new[] { "## Status", "## Context", "## Decision", "## Consequences" } },
            new object[] { "docs/adr/002-jwt-cookie-migration.md", new[] { "## Status", "## Context", "## Decision", "## Consequences" } },
            new object[] { "docs/adr/003-yarp-retention.md", new[] { "## Status", "## Context", "## Decision", "## Consequences" } },
            new object[] { "docs/adr/004-angular-ngmodules.md", new[] { "## Status", "## Context", "## Decision", "## Consequences" } },
            new object[] { "docs/adr/005-forge-shipping-cicd.md", new[] { "## Status", "## Context", "## Decision", "## Consequences" } },
            new object[] { "docs/adr/006-service-layer-refactor.md", new[] { "## Status", "## Context", "## Decision", "## Consequences" } },
            new object[] { "docs/compliance/data-classification.md", new[] { "T1", "T4", "Entity Mapping" } },
            new object[] { "docs/compliance/hipaa-checklist.md", new[] { "Compliant", "In Progress", "Pending", "secrets-management.md" } },
            new object[] { "docs/secrets-management.md", new[] { "90-day", "Rotation Policy", "Emergency Rotation", "Audit Trail Requirements", "JWT Signing Key Rotation", "MSSQL SA Password Rotation", "SMTP Credentials Rotation", "Google OAuth Credentials Rotation" } },
            new object[] { "docs/compliance/data-subject-rights.md", new[] { "30-day", "Breach Notification", "Pending" } },
            new object[] { "CONTRIBUTING.md", new[] { "Local Setup", "Coding Standards", "Pull Request Process" } },
        };

    [Theory]
    [MemberData(nameof(RequiredDocumentationFiles))]
    public void RequiredDocumentationFile_ExistsWithExpectedSections(string relativePath, string[] requiredSections)
    {
        var fullPath = Path.Combine(RepoRoot, relativePath);

        File.Exists(fullPath).Should().BeTrue(because: $"WO-030/WO-031 require {relativePath}");

        var content = File.ReadAllText(fullPath);
        foreach (var section in requiredSections)
        {
            content.Should().Contain(section, because: $"{relativePath} must include section or keyword '{section}'");
        }
    }

    [Fact]
    public void Readme_IncludesArchitectureSetupAndDeploymentSections()
    {
        var readmePath = Path.Combine(RepoRoot, "README.md");
        File.Exists(readmePath).Should().BeTrue();

        var readme = File.ReadAllText(readmePath);
        readme.Should().Contain("Architecture Overview");
        readme.Should().Contain("Setup (Quick Reference)");
        readme.Should().Contain("Deployment");
        readme.Should().Contain("docs/adr/");
    }

    [Fact]
    public void AdrDirectory_ContainsSixArchitectureDecisionRecords()
    {
        var adrDirectory = Path.Combine(RepoRoot, "docs", "adr");
        Directory.Exists(adrDirectory).Should().BeTrue();

        var adrFiles = Directory.GetFiles(adrDirectory, "*.md");
        adrFiles.Should().HaveCount(6, because: "WO-023 adds ADR-006 for service layer refactor verification");
    }

    [Fact]
    public void ComplianceDirectory_ContainsRequiredHipaaAndPrivacyDocs()
    {
        var complianceDirectory = Path.Combine(RepoRoot, "docs", "compliance");
        Directory.Exists(complianceDirectory).Should().BeTrue();

        var expectedFiles = new[]
        {
            "data-classification.md",
            "hipaa-checklist.md",
            "data-subject-rights.md"
        };

        foreach (var file in expectedFiles)
        {
            File.Exists(Path.Combine(complianceDirectory, file)).Should().BeTrue(
                because: $"WO-031 requires docs/compliance/{file}");
        }
    }
}
