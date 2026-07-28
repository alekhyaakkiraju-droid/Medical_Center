using FluentAssertions;

namespace AngularApi.Tests.Smoke;

public class SmokeTestScriptTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void SmokeTestScripts_ExistAndDefineExpandedCriticalFlows()
    {
        var pipelineSmoke = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "smoke-tests.sh"));
        var e2eSmoke = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "run-e2e-smoke.sh"));
        var patientJourney = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "e2e-patient-journey.sh"));
        var ssrSmoke = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "ssr-smoke-tests.sh"));
        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));

        pipelineSmoke.Should().Contain("/health");
        pipelineSmoke.Should().Contain("SMOKE_BASE_URL");
        pipelineSmoke.Should().Contain("SMOKE_API_URL");
        pipelineSmoke.Should().Contain("FRONTEND_URL");
        pipelineSmoke.Should().Contain("Smoke Test 4: Image-bearing Team page");
        pipelineSmoke.Should().Contain("Smoke Test 5: Image-bearing Gallery page");
        pipelineSmoke.Should().Contain("Smoke Test 6: Unauthorized API rejection");
        pipelineSmoke.Should().Contain("Smoke Test 7: SPA navigation uses routerLink");
        pipelineSmoke.Should().Contain("/pages/team");
        pipelineSmoke.Should().Contain("/pages/gallery");
        pipelineSmoke.Should().Contain("routerlink");
        pipelineSmoke.Should().Contain("app-root");
        pipelineSmoke.Should().Contain("120");

        e2eSmoke.Should().Contain("Smoke Test 1");
        e2eSmoke.Should().Contain("Smoke Test 2");
        e2eSmoke.Should().Contain("Smoke Test 3");
        e2eSmoke.Should().Contain("Smoke Test 4: Image-bearing Team page");
        e2eSmoke.Should().Contain("Smoke Test 5: Image-bearing Gallery page");
        e2eSmoke.Should().Contain("Smoke Test 6: Unauthorized API returns structured JSON");
        e2eSmoke.Should().Contain("Smoke Test 7: SPA navigation uses routerLink");
        e2eSmoke.Should().Contain("401");
        e2eSmoke.Should().Contain("application/json");
        e2eSmoke.Should().Contain("/pages/gallery");
        e2eSmoke.Should().Contain("routerlink");

        patientJourney.Should().Contain("Patient Journey Step 1");
        patientJourney.Should().Contain("Patient Journey Step 2");
        patientJourney.Should().Contain("Patient Journey Step 3");
        patientJourney.Should().Contain("Patient Journey Step 4");
        patientJourney.Should().Contain("Patient Journey Step 5");
        patientJourney.Should().Contain("Patient Journey Step 6");
        patientJourney.Should().Contain("Patient Journey Step 7");
        patientJourney.Should().Contain("Patient Journey Step 8");
        patientJourney.Should().Contain("Patient Journey Step 9");
        patientJourney.Should().Contain("Patient Journey Step 10");
        patientJourney.Should().Contain("Patient Journey Step 11");
        patientJourney.Should().Contain("CreateAppointmentDTO");
        patientJourney.Should().Contain("MedicalCenterDoctorAvailabilities");
        patientJourney.Should().Contain("MailHog");
        patientJourney.Should().Contain("/Doctors/");
        patientJourney.Should().Contain("/bookings");
        patientJourney.Should().Contain("antiforgery-token");
        patientJourney.Should().Contain("X-XSRF-TOKEN");
        patientJourney.Should().Contain("/api/Account/me");
        patientJourney.Should().Contain("/Appointments/patient/");
        patientJourney.Should().Contain("patient.alice@uat.careshift.local");
        patientJourney.Should().Contain("UatSeed123!");

        pipeline.Should().Contain("Expanded Smoke Tests");
        pipeline.Should().Contain("run-e2e-smoke.sh");
        pipeline.Should().Contain("Staging Smoke Tests");
        pipeline.Should().Contain("Patient Journey E2E");
        pipeline.Should().Contain("e2e-patient-journey.sh");
        pipeline.Should().Contain("./scripts/smoke-tests.sh");

        ssrSmoke.Should().Contain("WO-059");
        ssrSmoke.Should().Contain("/api/nonexistent");
        ssrSmoke.Should().Contain("/pages/about-us");
    }

    [Fact]
    public void PatientJourneyScript_ExistsAndDefinesAuthenticatedFlowSteps()
    {
        var patientJourneyPath = Path.Combine(RepoRoot, "scripts", "e2e-patient-journey.sh");
        File.Exists(patientJourneyPath).Should().BeTrue();

        var patientJourney = File.ReadAllText(patientJourneyPath);
        patientJourney.Should().Contain("login");
        patientJourney.Should().Contain("Profile");
        patientJourney.Should().Contain("Appointment");
    }

    [Fact]
    public void ForgePipeline_StagingEnvironmentOrdersExpandedSmokeAndPatientJourneyAfterBasicSmoke()
    {
        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));
        var devBlockStart = pipeline.IndexOf("dev:", StringComparison.Ordinal);
        var stagingBlockStart = pipeline.IndexOf("staging:", StringComparison.Ordinal);
        var productionBlockStart = pipeline.IndexOf("production:", StringComparison.Ordinal);
        var devSteps = pipeline[devBlockStart..stagingBlockStart];
        var stagingSteps = pipeline[stagingBlockStart..productionBlockStart];

        var smokeIndex = stagingSteps.IndexOf("- Staging Smoke Tests", StringComparison.Ordinal);
        var expandedIndex = stagingSteps.IndexOf("- Expanded Smoke Tests", StringComparison.Ordinal);
        var staticAssetIndex = stagingSteps.IndexOf("- Static Asset Verification", StringComparison.Ordinal);
        var patientIndex = stagingSteps.IndexOf("- Patient Journey E2E", StringComparison.Ordinal);
        var dastIndex = stagingSteps.IndexOf("- DAST Scan", StringComparison.Ordinal);

        smokeIndex.Should().BeGreaterThan(-1);
        expandedIndex.Should().BeGreaterThan(smokeIndex);
        staticAssetIndex.Should().BeGreaterThan(expandedIndex);
        patientIndex.Should().BeGreaterThan(staticAssetIndex);
        dastIndex.Should().BeGreaterThan(patientIndex);

        devSteps.Should().NotContain("Expanded Smoke Tests");
        devSteps.Should().NotContain("Patient Journey E2E");
    }

    [Fact]
    public void SmokeTestScripts_AreExecutable()
    {
        var pipelineSmoke = new FileInfo(Path.Combine(RepoRoot, "scripts", "smoke-tests.sh"));
        var e2eSmoke = new FileInfo(Path.Combine(RepoRoot, "scripts", "run-e2e-smoke.sh"));
        var patientJourney = new FileInfo(Path.Combine(RepoRoot, "scripts", "e2e-patient-journey.sh"));

        pipelineSmoke.Exists.Should().BeTrue();
        e2eSmoke.Exists.Should().BeTrue();
        patientJourney.Exists.Should().BeTrue();
    }

    [Fact]
    public void StaticAssetVerificationScript_ExistsAndChecksImageAssets()
    {
        var scriptPath = Path.Combine(RepoRoot, "scripts", "check-static-assets.sh");
        File.Exists(scriptPath).Should().BeTrue(because: "WO-022 requires a static asset verification script");

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("curl");
        script.Should().Contain("http_code");
        script.Should().Contain("image");
        script.Should().Contain("/pages/about-us");
        script.Should().Contain("/pages/service");
        script.Should().Contain("/pages/contact");
        script.Should().Contain("exit 1");

        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));
        pipeline.Should().Contain("Static Asset Verification");
        pipeline.Should().Contain("check-static-assets.sh");
    }

    [Fact]
    public void JourneyPublicSmokeScript_ExistsAndValidatesPublicRoutesAndBranding()
    {
        var scriptPath = Path.Combine(RepoRoot, "scripts", "journey-smoke-public.sh");
        File.Exists(scriptPath).Should().BeTrue(because: "WO-051 requires a public page journey smoke script");

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("SMOKE_BASE_URL");
        script.Should().Contain("/pages/about-us");
        script.Should().Contain("/pages/contact");
        script.Should().Contain("/pages/service");
        script.Should().Contain("/pages/blog");
        script.Should().Contain("/pages/gallery");
        script.Should().Contain("/pages/team");
        script.Should().Contain("Lorem ipsum");
        script.Should().Contain("PrimeCare");
        script.Should().Contain("Modamba");
        script.Should().Contain("CareShift");
        script.Should().Contain("exit 1");

        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));
        pipeline.Should().Contain("Public Page Journey Smoke Tests");
        pipeline.Should().Contain("journey-smoke-public.sh");
    }

    [Fact]
    public void JourneyAuthSmokeScript_ExistsAndDefinesRoleBasedFlows()
    {
        var scriptPath = Path.Combine(RepoRoot, "scripts", "journey-smoke-auth.sh");
        File.Exists(scriptPath).Should().BeTrue(because: "WO-052 requires an authenticated journey smoke script");

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("SMOKE_API_URL");
        script.Should().Contain("admin@uat.careshift.local");
        script.Should().Contain("dr.smith@uat.careshift.local");
        script.Should().Contain("patient.alice@uat.careshift.local");
        script.Should().Contain("UatSeed123!");
        script.Should().Contain("antiforgery-token");
        script.Should().Contain("X-XSRF-TOKEN");
        script.Should().Contain("/Doctors/");
        script.Should().Contain("/Appointments/patient/");
        script.Should().Contain("exit 1");

        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));
        pipeline.Should().Contain("Authenticated Journey Smoke Tests");
        pipeline.Should().Contain("journey-smoke-auth.sh");
    }

    [Fact]
    public void JourneyAssetsSmokeScript_ExistsAndCrawlsPublicPageAssets()
    {
        var scriptPath = Path.Combine(RepoRoot, "scripts", "journey-smoke-assets.sh");
        File.Exists(scriptPath).Should().BeTrue(because: "WO-053 requires a static asset journey smoke script");

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("SMOKE_BASE_URL");
        script.Should().Contain("/pages/gallery");
        script.Should().Contain("/pages/team");
        script.Should().Contain("localhost");
        script.Should().Contain("http_code");
        script.Should().Contain("exit 1");

        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));
        pipeline.Should().Contain("Static Asset Journey Smoke Tests");
        pipeline.Should().Contain("journey-smoke-assets.sh");
    }

    [Fact]
    public void OpenApiGenerationScripts_ContainOutputValidation()
    {
        var generateOpenApi = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "generate-openapi.sh"));
        var generateApiTypes = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "generate-api-types.sh"));
        generateOpenApi.Should().Contain("exit 1").And.Contain("json.tool").And.Contain("ERROR:");
        generateApiTypes.Should().Contain("exit 1").And.Contain("ERROR:").And.Contain("export statements");
    }
    [Fact]
    public void ForgePipeline_FrontendBuildRunsGenerateApiTypesBeforeNgBuild()
    {
        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));
        pipeline.Should().Contain("- name: Generate OpenAPI Spec");
        pipeline.Should().Contain("npm run build -- --configuration production");
        pipeline.Should().NotContain("./node_modules/.bin/ng build --configuration production");
    }
}
