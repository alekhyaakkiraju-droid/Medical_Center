using System.Reflection;
using AngularApi.Controllers;
using AngularApi.DTO;
using AngularApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Tests.Architecture;

public class EntityLeakageArchitectureTests
{
    private static readonly Assembly ControllersAssembly = typeof(DoctorsController).Assembly;
    private const string ModelsNamespace = "AngularApi.Models";

    /// <summary>
    /// Legacy POST/PUT actions that still return EF entities — tracked for removal in follow-up stories.
    /// GET actions must not appear here; WO-006 eliminated DoctorsController.GetDoctor entity leakage.
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

    [Fact]
    public void ControllerActions_ShouldNotReturnEntityModels()
    {
        var violations = GetControllerActionEntityReturnViolations().ToList();
        var unexpected = violations
            .Where(v => !KnownLegacyEntityReturnActions.Contains(v))
            .ToList();
        var resolved = KnownLegacyEntityReturnActions
            .Where(known => !violations.Contains(known))
            .ToList();

        unexpected.Should().BeEmpty(
            because: "controller actions must not return EF Core entity types from {0}. New violations: {1}",
            ModelsNamespace,
            string.Join("; ", unexpected));

        resolved.Should().BeEmpty(
            because: "legacy allowlist entries should be removed once entity returns are fixed. Resolved: {0}",
            string.Join("; ", resolved));
    }

    [Fact]
    public void DoctorsController_GetDoctor_ShouldReturnDoctorDetailDto()
    {
        var getDoctor = typeof(DoctorsController).GetMethod(nameof(DoctorsController.GetDoctor));
        getDoctor.Should().NotBeNull();

        var returnType = UnwrapReturnType(getDoctor!.ReturnType);
        returnType.Should().Be(typeof(DoctorDetailDTO),
            because: "GetDoctor must return DoctorDetailDTO to prevent entity leakage");
    }

    private static IEnumerable<string> GetControllerActionEntityReturnViolations() =>
        ControllersAssembly.GetTypes()
            .Where(type => type.IsClass
                           && !type.IsAbstract
                           && typeof(ControllerBase).IsAssignableFrom(type)
                           && type.Namespace == typeof(DoctorsController).Namespace)
            .SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsControllerActionMethod)
                .SelectMany(method =>
                {
                    var entityTypes = GetEntityReturnTypes(method.ReturnType);
                    return entityTypes.Select(entityType =>
                        $"{controller.Name}.{method.Name} -> {entityType.Name}");
                }))
            .Select(violation => violation.Split(" -> ")[0]);

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

    private static IEnumerable<Type> GetEntityReturnTypes(Type returnType)
    {
        var unwrapped = UnwrapReturnType(returnType);

        if (IsEntityModelType(unwrapped))
        {
            yield return unwrapped;
            yield break;
        }

        if (unwrapped.IsGenericType)
        {
            foreach (var typeArg in unwrapped.GetGenericArguments())
            {
                if (IsEntityModelType(typeArg))
                {
                    yield return typeArg;
                }
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

    private static bool IsEntityModelType(Type type) =>
        type.Namespace == ModelsNamespace
        && type.IsClass
        && type != typeof(MedicalCenterDbContext);
}
