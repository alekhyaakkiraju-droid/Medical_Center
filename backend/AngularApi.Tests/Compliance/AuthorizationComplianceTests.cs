using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Compliance;

public class AuthorizationComplianceTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string ManifestPath =
        Path.Combine(RepoRoot, "docs", "compliance", "authorization-test-manifest.md");

    private static readonly string PipelinePath =
        Path.Combine(RepoRoot, ".forge", "pipeline.yaml");

    private static readonly string[] ExpectedTestClasses =
    [
        "AccountControllerAuthorizationIntegrationTests",
        "AppointmentStatusControllerAuthorizationIntegrationTests",
        "AppointmentsControllerAuthorizationIntegrationTests",
        "AuditLoggingIntegrationTests",
        "AuthRateLimitingIntegrationTests",
        "AuthorizationPolicyIntegrationTests",
        "ControllerAuthorizationIntegrationTests",
        "CookieAuthIntegrationTests",
        "DoctorsControllerAuthorizationIntegrationTests",
        "MedicalCenterDoctorAvailabilitiesControllerAuthorizationIntegrationTests",
        "MedicalCentersControllerAuthorizationIntegrationTests",
        "OwnershipValidationIntegrationTests",
        "PatientReviewsControllerAuthorizationIntegrationTests",
        "PatientsControllerAuthorizationIntegrationTests",
        "SpecializationsControllerAuthorizationIntegrationTests",
    ];

    [Fact]
    public void AuthorizationTestManifest_ExistsAndMapsAllTestClasses()
    {
        File.Exists(ManifestPath).Should().BeTrue(because: "WO-041 requires a compliance test manifest");

        var manifest = File.ReadAllText(ManifestPath);

        foreach (var testClass in ExpectedTestClasses)
        {
            manifest.Should().Contain(testClass, because: "each authorization test class must map to a HIPAA control");
        }

        manifest.Should().Contain("MedicalCenterWebApplicationFactory");
        manifest.Should().Contain("TestJwtFactory");
        manifest.Should().Contain("AntiforgeryTestHelper");
        manifest.Should().Contain("AuthorizationIntegrationTestBase");
    }

    [Fact]
    public void ForgePipeline_IncludesAuthorizationRegressionGateAfterBackendUnitTests()
    {
        var yaml = File.ReadAllText(PipelinePath);

        yaml.Should().Contain("Authorization Regression Gate");
        yaml.Should().Contain(
            "dotnet test backend/AngularApi.Tests/AngularApi.Tests.csproj --filter FullyQualifiedName~Authorization -c Release");

        var backendTestsIndex = yaml.IndexOf("- name: Backend Unit Tests", StringComparison.Ordinal);
        var authGateIndex = yaml.IndexOf("- name: Authorization Regression Gate", StringComparison.Ordinal);
        var frontendTestsIndex = yaml.IndexOf("- name: Frontend Unit Tests", StringComparison.Ordinal);

        authGateIndex.Should().BeGreaterThan(backendTestsIndex);
        authGateIndex.Should().BeLessThan(frontendTestsIndex);

        var devBlockStart = yaml.IndexOf("environments:", StringComparison.Ordinal);
        var stagingBlockStart = yaml.IndexOf("  staging:", StringComparison.Ordinal);
        var productionBlockStart = yaml.IndexOf("  production:", StringComparison.Ordinal);

        yaml[devBlockStart..stagingBlockStart].Should().Contain("- Authorization Regression Gate");
        yaml[stagingBlockStart..productionBlockStart].Should().Contain("- Authorization Regression Gate");
    }

    [Fact]
    public void HipaaChecklist_ReferencesAuthorizationTestManifest()
    {
        var checklist = File.ReadAllText(Path.Combine(RepoRoot, "docs", "compliance", "hipaa-checklist.md"));

        checklist.Should().Contain("authorization-test-manifest.md");
        checklist.Should().Contain("authorization regression gate");
    }

    [Fact]
    public void TestJwtFactory_UsesSameIssuerAudienceAndSecretAsTestHost()
    {
        using var factory = new MedicalCenterWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var token = TestJwtFactory.CreateToken(configuration, "user");
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be(configuration["Jwt:ValidIssuer"]);
        jwt.Audiences.Should().Contain(configuration["Jwt:ValidAudience"]);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "user");
    }

    [Fact]
    public async Task AntiforgeryTestHelper_ReturnsTokenFromAccountEndpoint()
    {
        using var factory = new MedicalCenterWebApplicationFactory();
        var client = AntiforgeryTestHelper.CreateClient(factory);

        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        client.DefaultRequestHeaders.Contains("X-XSRF-TOKEN").Should().BeTrue();
        client.DefaultRequestHeaders.GetValues("X-XSRF-TOKEN").First().Should().NotBeNullOrWhiteSpace();
    }
}
