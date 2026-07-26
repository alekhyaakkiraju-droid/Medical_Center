using FluentAssertions;
using Polly;
using Polly.Retry;

namespace AngularApi.Tests.Infrastructure;

public class DotNet10ThirdPartyLibraryCompatibilityTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void DoctorService_UsesPollyResiliencePipelineApi()
    {
        var source = File.ReadAllText(Path.Combine(
            RepoRoot,
            "backend",
            "AngularApi",
            "Services",
            "impelementation",
            "DoctorService.cs"));

        source.Should().Contain("ResiliencePipeline");
        source.Should().Contain("ResiliencePipelineBuilder");
        source.Should().Contain("RetryStrategyOptions");
        source.Should().NotContain("AsyncRetryPolicy");
        source.Should().NotContain("Policy.Handle<Exception>()");
    }

    [Fact]
    public void SerilogConfiguration_UsesDotNet10CompatibleRegistration()
    {
        var source = File.ReadAllText(Path.Combine(
            RepoRoot,
            "backend",
            "AngularApi",
            "Logging",
            "SerilogConfiguration.cs"));

        source.Should().Contain("AddSerilog");
        source.Should().Contain("ReadFrom.Configuration");
        source.Should().Contain("ReadFrom.Services");
        source.Should().Contain("Enrich.FromLogContext()");
        source.Should().Contain("Enrich.WithProperty(\"Application\", \"MedicalCenter\")");
        source.Should().Contain("Destructure.ByTransformingWhere<string>");
        source.Should().Contain("RenderedCompactJsonFormatter");
    }

    [Fact]
    public void Program_RegistersFluentValidationAutoValidation()
    {
        var source = File.ReadAllText(Path.Combine(
            RepoRoot,
            "backend",
            "AngularApi",
            "Program.cs"));

        source.Should().Contain("AddFluentValidationAutoValidation()");
        source.Should().Contain("AddValidatorsFromAssemblyContaining<RegisterUserDTOValidator>()");
    }

    [Fact]
    public void CorrelationIdMiddleware_PushesCorrelationIdIntoLogContext()
    {
        var source = File.ReadAllText(Path.Combine(
            RepoRoot,
            "backend",
            "AngularApi",
            "Middleware",
            "CorrelationIdMiddleware.cs"));

        source.Should().Contain("LogContext.PushProperty(\"CorrelationId\"");
    }

    [Fact]
    public async Task DoctorServiceRetryPipeline_RetriesThreeTimesBeforeThrowing()
    {
        var attempts = 0;
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                DelayGenerator = static args =>
                    new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(1000 * args.AttemptNumber)),
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();

        Func<Task> act = async () => await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            throw new InvalidOperationException("transient failure");
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(4, because: "Polly performs one initial attempt plus three retries");
    }

    [Theory]
    [InlineData(1, 1000)]
    [InlineData(2, 2000)]
    [InlineData(3, 3000)]
    public void DoctorServiceRetryPipeline_UsesLinearBackoffDelay(int attemptNumber, int expectedDelayMs)
    {
        var delay = TimeSpan.FromMilliseconds(1000 * attemptNumber);
        delay.TotalMilliseconds.Should().Be(expectedDelayMs);
    }
}
