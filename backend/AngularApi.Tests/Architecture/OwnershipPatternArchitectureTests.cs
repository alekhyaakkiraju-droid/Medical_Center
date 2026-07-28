using AngularApi.Controllers;
using AngularApi.Contracts.Services.Interfaces;
using FluentAssertions;
using NetArchTest.Rules;

namespace AngularApi.Tests.Architecture;

public class OwnershipPatternArchitectureTests
{
    [Fact]
    public void Controllers_ShouldNotDependOnIOwnershipValidator()
    {
        var result = Types.InAssembly(typeof(DoctorsController).Assembly)
            .That()
            .ResideInNamespace("AngularApi.Controllers")
            .Should()
            .NotHaveDependencyOn(nameof(IOwnershipValidator))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "controllers must use [ValidateOwnership] instead of injecting IOwnershipValidator directly. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }
}
