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
        var pipeline = File.ReadAllText(Path.Combine(RepoRoot, ".forge", "pipeline.yaml"));

        pipelineSmoke.Should().Contain("/health");
        pipelineSmoke.Should().Contain("SMOKE_BASE_URL");
        pipelineSmoke.Should().Contain("SMOKE_API_URL");
        pipelineSmoke.Should().Contain("Image-bearing page validation");
        pipelineSmoke.Should().Contain("/pages/team");
        pipelineSmoke.Should().Contain("app-root");
        pipelineSmoke.Should().Contain("antiforgery-token");
        pipelineSmoke.Should().Contain("120");

        e2eSmoke.Should().Contain("Smoke Test 1");
        e2eSmoke.Should().Contain("Smoke Test 2");
        e2eSmoke.Should().Contain("Smoke Test 3");
        e2eSmoke.Should().Contain("Smoke Test 4");
        e2eSmoke.Should().Contain("Smoke Test 5");
        e2eSmoke.Should().Contain("Smoke Test 6");
        e2eSmoke.Should().Contain("401");
        e2eSmoke.Should().Contain("application/json");
        e2eSmoke.Should().Contain("antiforgery-token");

        pipeline.Should().Contain("Staging E2E Tests");
        pipeline.Should().Contain("run-e2e-smoke.sh");
        pipeline.Should().Contain("Staging Smoke Tests");
    }

    [Fact]
    public void SmokeTestScripts_AreExecutable()
    {
        var pipelineSmoke = new FileInfo(Path.Combine(RepoRoot, "scripts", "smoke-tests.sh"));
        var e2eSmoke = new FileInfo(Path.Combine(RepoRoot, "scripts", "run-e2e-smoke.sh"));

        pipelineSmoke.Exists.Should().BeTrue();
        e2eSmoke.Exists.Should().BeTrue();
    }
}
