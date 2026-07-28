using System.Reflection;
using AngularApi.Controllers;
using AngularApi.Contracts.DTO;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;

namespace AngularApi.Tests.Architecture;

/// <summary>
/// Enforces inward dependency direction after shared-contract extraction (WO-013 through WO-015).
/// Contracts must never reference AngularApi; controllers must expose DTOs from Contracts.
/// </summary>
public class CircularDependencyArchitectureTests
{
    private const string ContractsDtoNamespace = "AngularApi.Contracts.DTO";
    private const string ContractsModelsNamespace = "AngularApi.Contracts.Models";

    private static readonly Assembly ContractsAssembly = typeof(AppointmentDTO).Assembly;
    private static readonly Assembly ApiAssembly = typeof(DoctorsController).Assembly;

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

    /// <summary>
    /// Legacy controller actions still returning entity types from Contracts.Models — tracked for remediation.
    /// </summary>
    private static readonly HashSet<string> KnownLegacyEntityReturnActions =
    [
        $"{nameof(AppointmentsController)}.{nameof(AppointmentsController.GetAppointment)}",
        $"{nameof(AppointmentsController)}.{nameof(AppointmentsController.PostAppointment)}",
        $"{nameof(AppointmentStatusController)}.{nameof(AppointmentStatusController.PostAppointmentStatus)}",
        $"{nameof(DoctorsController)}.{nameof(DoctorsController.PostDoctor)}",
        $"{nameof(MedicalCentersController)}.{nameof(MedicalCentersController.PostMedicalCenter)}",
        $"{nameof(MedicalCenterDoctorAvailabilitiesController)}.{nameof(MedicalCenterDoctorAvailabilitiesController.PostMedicalCenterDoctorAvailability)}",
        $"{nameof(PatientReviewsController)}.{nameof(PatientReviewsController.PostPatientReview)}",
        $"{nameof(SpecializationsController)}.{nameof(SpecializationsController.PostSpecialization)}",
    ];

    /// <summary>
    /// Test 1: AngularApi.Contracts must not reference the AngularApi implementation assembly.
    /// </summary>
    [Fact]
    public void ContractsAssembly_ShouldNotReference_AngularApiAssembly()
    {
        var apiAssemblyName = ApiAssembly.GetName().Name;

        var referencedAssemblyNames = ContractsAssembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToList();

        referencedAssemblyNames.Should().NotContain(apiAssemblyName,
            because: "dependency direction must be AngularApi -> Contracts, never the reverse");
    }

    /// <summary>
    /// Test 2: Contracts contains only interfaces, enums, records, and simple data shapes.
    /// </summary>
    [Fact]
    public void ContractsAssembly_ShouldContainOnly_InterfacesEnumsDtosAndValueTypes()
    {
        var violations = ContractsAssembly.GetTypes()
            .Where(type => type is { IsNestedPrivate: false })
            .Where(type => type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is null)
            .Where(type => !IsAllowedContractType(type))
            .Select(type => type.FullName ?? type.Name)
            .ToList();

        violations.Should().BeEmpty(
            because: "Contracts must contain only interfaces, enums, records, or property-only classes. Violations: {0}",
            string.Join(", ", violations));
    }

    /// <summary>
    /// Test 3: Contracts must not depend on Entity Framework Core.
    /// </summary>
    [Fact]
    public void ContractsAssembly_ShouldNotReference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .That()
            .ResideInNamespace("AngularApi.Contracts")
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "AngularApi.Contracts must remain free of EF Core types. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    /// <summary>
    /// Test 4: Controller actions must return Contract DTOs, not entity models (with documented legacy exclusions).
    /// </summary>
    [Fact]
    public void Controllers_ShouldReturn_OnlyContractDtos_NotEntities()
    {
        var violations = GetControllerEntityReturnViolations().ToList();
        var unexpected = violations
            .Where(violation => !KnownLegacyEntityReturnActions.Contains(violation))
            .ToList();

        unexpected.Should().BeEmpty(
            because: "controller actions must return DTOs from {0}, not entity types from {1}. New violations: {2}",
            ContractsDtoNamespace,
            ContractsModelsNamespace,
            string.Join("; ", unexpected));
    }

    [Fact]
    public void Contracts_ShouldNotDependOnAngularApiImplementationNamespaces()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .That()
            .ResideInNamespace("AngularApi.Contracts")
            .Should()
            .NotHaveDependencyOnAny(ForbiddenAngularApiNamespaces)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "AngularApi.Contracts must not depend on AngularApi implementation namespaces. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    [Fact]
    public void Contracts_ShouldNotDependOnAngularApiTestsNamespace()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .That()
            .ResideInNamespace("AngularApi.Contracts")
            .Should()
            .NotHaveDependencyOn("AngularApi.Tests")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "AngularApi.Contracts must not depend on AngularApi.Tests. Violating types: {0}",
            string.Join(", ", result.FailingTypes ?? []));
    }

    private static bool IsAllowedContractType(Type type)
    {
        if (type.Namespace is null || !type.Namespace.StartsWith("AngularApi.Contracts", StringComparison.Ordinal))
        {
            return true;
        }

        if (type.IsInterface || type.IsEnum)
        {
            return true;
        }

        if (IsRecordType(type))
        {
            return true;
        }

        if (type is { IsAbstract: true, IsSealed: false })
        {
            return true;
        }

        if (type.IsAbstract)
        {
            return true;
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var disallowedMethods = methods
            .Where(method => !method.IsSpecialName)
            .Where(method => !method.Name.StartsWith('<')) // exclude compiler-generated state machine helpers
            .ToList();

        if (disallowedMethods.Count > 0)
        {
            return false;
        }

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Length > 0
               || type.GetFields(BindingFlags.Public | BindingFlags.Instance).Length > 0
               || type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0;
    }

    private static bool IsRecordType(Type type) =>
        type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;

    private static IEnumerable<string> GetControllerEntityReturnViolations() =>
        ApiAssembly.GetTypes()
            .Where(type => type.IsClass
                           && !type.IsAbstract
                           && typeof(ControllerBase).IsAssignableFrom(type)
                           && type.Namespace == typeof(DoctorsController).Namespace)
            .SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsControllerActionMethod)
                .SelectMany(method => GetDisallowedReturnTypes(method.ReturnType)
                    .Select(_ => $"{controller.Name}.{method.Name}")));

    private static bool IsControllerActionMethod(MethodInfo method)
    {
        if (method.GetCustomAttribute<NonActionAttribute>() != null)
        {
            return false;
        }

        return method.GetCustomAttribute<HttpGetAttribute>() != null
               || method.GetCustomAttribute<HttpPostAttribute>() != null
               || method.GetCustomAttribute<HttpPutAttribute>() != null
               || method.GetCustomAttribute<HttpDeleteAttribute>() != null
               || method.GetCustomAttribute<HttpPatchAttribute>() != null;
    }

    private static IEnumerable<Type> GetDisallowedReturnTypes(Type returnType)
    {
        foreach (var candidate in ExtractPayloadTypes(returnType))
        {
            if (candidate.Namespace == ContractsModelsNamespace && candidate.IsClass)
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<Type> ExtractPayloadTypes(Type returnType)
    {
        var unwrapped = UnwrapReturnType(returnType);

        if (unwrapped == typeof(void)
            || unwrapped == typeof(IActionResult)
            || unwrapped == typeof(ActionResult)
            || unwrapped.Namespace?.StartsWith("System", StringComparison.Ordinal) == true)
        {
            yield break;
        }

        if (unwrapped.Namespace == ContractsDtoNamespace)
        {
            yield break;
        }

        yield return unwrapped;

        if (!unwrapped.IsGenericType)
        {
            yield break;
        }

        foreach (var typeArgument in unwrapped.GetGenericArguments())
        {
            foreach (var nested in ExtractPayloadTypes(typeArgument))
            {
                yield return nested;
            }
        }
    }

    private static Type UnwrapReturnType(Type returnType)
    {
        while (returnType.IsGenericType)
        {
            var genericDefinition = returnType.GetGenericTypeDefinition();
            if (genericDefinition == typeof(Task<>)
                || genericDefinition == typeof(ActionResult<>)
                || genericDefinition == typeof(ValueTask<>))
            {
                returnType = returnType.GetGenericArguments()[0];
                continue;
            }

            break;
        }

        return returnType;
    }
}
