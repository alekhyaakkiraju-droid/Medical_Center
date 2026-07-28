using AngularApi.Contracts.DTO;
using FluentAssertions;
using NetArchTest.Rules;

namespace AngularApi.Tests.Architecture;

public class CircularDependencyArchitectureTests
{
    private static readonly string[] ForbiddenAngularApiNamespaces =
    [
        "AngularApi.Controllers",
        "AngularApi.Services",
        "AngularApi.Models",
        "AngularApi.Filters",
        "AngularApi.Validators",
        "AngularApi.Middleware",
        "AngularApi.Infrastructure",
        "AngularApi.Migrations",
        "AngularApi.Options",
        "AngularApi.Logging",
    ];

    [Fact]
    public void Contracts_ShouldNotDependOnAngularApiImplementationNamespaces()
    {
        var result = Types.InAssembly(typeof(AppointmentDTO).Assembly)
            .That()
            .ResideInNamespace("AngularApi.Contracts")
            .Should()
            .NotHaveDependencyOnAny(ForbiddenAngularApiNamespaces)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "AngularApi.Contracts must not depend on AngularApi implementation namespaces to avoid circular project dependencies. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    [Fact]
    public void Contracts_ShouldNotDependOnAngularApiTestsNamespace()
    {
        var result = Types.InAssembly(typeof(AppointmentDTO).Assembly)
            .That()
            .ResideInNamespace("AngularApi.Contracts")
            .Should()
            .NotHaveDependencyOn("AngularApi.Tests")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "AngularApi.Contracts must not depend on AngularApi.Tests. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }
}
