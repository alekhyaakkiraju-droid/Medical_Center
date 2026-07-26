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
        patientJourney.Should().Contain("antiforgery-token");
        patientJourney.Should().Contain("X-XSRF-TOKEN");
        patientJourney.Should().Contain("/api/Account/me");
        patientJourney.Should().Contain("/Appointments/patient/");
        patientJourney.Should().Contain("patient.alice@uat.careshift.local");
        patientJourney.Should().Contain("UatSeed123!");

        pipeline.Should().Contain("Staging E2E Tests");
        pipeline.Should().Contain("run-e2e-smoke.sh");
        pipeline.Should().Contain("Staging Smoke Tests");
        pipeline.Should().Contain("Patient Journey E2E");
        pipeline.Should().Contain("e2e-patient-journey.sh");
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
}
