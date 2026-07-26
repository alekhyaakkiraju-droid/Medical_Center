using FluentAssertions;

namespace AngularApi.Tests.Smoke;

public class SmokeTestScriptTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void SmokeTestScripts_ExistAndDefineThreeCriticalFlows()
    {
        var pipelineSmoke = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "smoke-tests.sh"));
        var e2eSmoke = File.ReadAllText(Path.Combine(RepoRoot, "scripts", "run-e2e-smoke.sh"));

        pipelineSmoke.Should().Contain("/health");
        pipelineSmoke.Should().Contain("SMOKE_BASE_URL");
        pipelineSmoke.Should().Contain("SMOKE_API_URL");
        e2eSmoke.Should().Contain("Smoke Test 1");
        e2eSmoke.Should().Contain("Smoke Test 2");
        e2eSmoke.Should().Contain("Smoke Test 3");
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
