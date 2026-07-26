using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class DotNet10TestSuiteValidationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string[] RequiredRegressionTestFiles =
    [
        "Controllers/AccountControllerTests.cs",
        "Controllers/AppointmentsControllerTests.cs",
        "Controllers/AppointmentsPaginationControllerTests.cs",
        "Controllers/DoctorsControllerTests.cs",
        "Controllers/MedicalCentersControllerTests.cs",
        "Controllers/PatientsControllerTests.cs",
        "Controllers/PatientReviewsControllerTests.cs",
        "Controllers/SpecializationsControllerTests.cs",
        "Services/AppointmentServiceTests.cs",
        "Services/AuditServiceTests.cs",
        "Services/AuthCookieServiceTests.cs",
        "Services/DevelopmentDataSeederTests.cs",
        "Services/DoctorServiceTests.cs",
        "Services/EmailServiceAsyncTests.cs",
        "Services/EmailTemplateServiceTests.cs",
        "Services/EnsureRolesCreatedAsyncTests.cs",
        "Services/JwtServiceTests.cs",
        "Services/OwnershipValidatorTests.cs",
        "Services/PatientServiceTests.cs",
        "Services/RefreshTokenServiceTests.cs",
        "Authorization/AuditLoggingIntegrationTests.cs",
        "Authorization/CookieAuthIntegrationTests.cs",
        "Authorization/OwnershipValidationIntegrationTests.cs",
        "Infrastructure/DockerfileConfigurationTests.cs",
        "Infrastructure/ForgePipelineConfigurationTests.cs",
        "Smoke/SmokeTestScriptTests.cs",
        "DTO/ListEndpointPayloadTests.cs",
        "DTO/PaginationParametersTests.cs",
        "DTO/QueryablePaginationExtensionsTests.cs",
        "DTO/QueryProjectionsTests.cs",
        "Models/AuditableEntityTests.cs",
        "Models/AuditLogAppendOnlyTests.cs",
    ];

    [Fact]
    public void DockerfileConfigurationTests_AssertDotNet10BaseImages()
    {
        var source = File.ReadAllText(Path.Combine(
            RepoRoot,
            "backend",
            "AngularApi.Tests",
            "Infrastructure",
            "DockerfileConfigurationTests.cs"));

        source.Should().Contain("mcr.microsoft.com/dotnet/aspnet:10.0");
        source.Should().Contain("mcr.microsoft.com/dotnet/sdk:10.0");
        source.Should().NotContain("mcr.microsoft.com/dotnet/aspnet:8.0");
        source.Should().NotContain("mcr.microsoft.com/dotnet/sdk:8.0");
    }

    [Fact]
    public void TestHostFactory_UsesInMemoryDatabaseAndSkipsRelationalMigrations()
    {
        var source = File.ReadAllText(Path.Combine(
            RepoRoot,
            "backend",
            "AngularApi.Tests",
            "Infrastructure",
            "MedicalCenterWebApplicationFactory.cs"));

        source.Should().Contain("UseInMemoryDatabase");
        source.Should().Contain("NoOpDatabaseMigrationRunner");
        source.Should().Contain("ConnectionStrings:SaPassword");
    }

    [Fact]
    public void UpgradeReportDocumentsDotNet10ValidationGate()
    {
        var reportPath = Path.Combine(RepoRoot, "docs", "dotnet10-test-upgrade-report.md");
        File.Exists(reportPath).Should().BeTrue(because: "WO-050 requires a .NET 10 upgrade test report");

        var report = File.ReadAllText(reportPath);
        report.Should().Contain(".NET 10");
        report.Should().Contain("AngularApi.Tests");
    }

    [Theory]
    [MemberData(nameof(RequiredRegressionTestFilePaths))]
    public void RequiredRegressionTestFilesExist(string relativePath)
    {
        File.Exists(Path.Combine(RepoRoot, "backend", "AngularApi.Tests", relativePath))
            .Should().BeTrue(because: $"WO-050 regression coverage requires {relativePath}");
    }

    public static IEnumerable<object[]> RequiredRegressionTestFilePaths =>
        RequiredRegressionTestFiles.Select(path => new object[] { path });
}
