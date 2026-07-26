using System.Reflection;
using System.Security.Claims;
using AngularApi.Controllers;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using AngularApi.Tests.Infrastructure;
using AngularApi.Validators;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Architecture;

public class ServiceLayerArchitectureTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private static readonly Assembly ApiAssembly = typeof(AppointmentStatusController).Assembly;

    private static readonly HashSet<string> RefactoredControllerNames =
    [
        nameof(AppointmentStatusController),
        nameof(SpecializationsController),
        nameof(MedicalCentersController),
        nameof(MedicalCenterDoctorAvailabilitiesController),
        nameof(PatientReviewsController),
    ];

    private static readonly Type[] RequiredServiceInterfaces =
    [
        typeof(IAppointmentStatusService),
        typeof(ISpecializationService),
        typeof(IMedicalCenterService),
        typeof(IMedicalCenterDoctorAvailabilityService),
        typeof(IPatientReviewService),
    ];

    private static readonly (Type DtoType, Type ValidatorType)[] RefactoredDtoValidators =
    [
        (typeof(CreateAppointmentStatusDTO), typeof(CreateAppointmentStatusDTOValidator)),
        (typeof(UpdateAppointmentStatusDTO), typeof(UpdateAppointmentStatusDTOValidator)),
        (typeof(CreateSpecializationDTO), typeof(CreateSpecializationDTOValidator)),
        (typeof(UpdateSpecializationDTO), typeof(UpdateSpecializationDTOValidator)),
        (typeof(CreateMedicalCenterDTO), typeof(CreateMedicalCenterDTOValidator)),
        (typeof(UpdateMedicalCenterDTO), typeof(UpdateMedicalCenterDTOValidator)),
        (typeof(CreateMedicalCenterDoctorAvailabilityDTO), typeof(CreateMedicalCenterDoctorAvailabilityDTOValidator)),
        (typeof(UpdateMedicalCenterDoctorAvailabilityDTO), typeof(UpdateMedicalCenterDoctorAvailabilityDTOValidator)),
        (typeof(CreatePatientReviewDTO), typeof(CreatePatientReviewDTOValidator)),
        (typeof(UpdatePatientReviewDTO), typeof(UpdatePatientReviewDTOValidator)),
    ];

    private readonly MedicalCenterWebApplicationFactory _factory;

    public ServiceLayerArchitectureTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void NoControllerInjectsDbContextDirectly()
    {
        var violations = GetControllerTypes()
            .SelectMany(controller => controller
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(constructor => constructor
                    .GetParameters()
                    .Where(parameter => parameter.ParameterType == typeof(MedicalCenterDbContext))
                    .Select(parameter => $"{controller.Name} constructor parameter '{parameter.Name}' injects MedicalCenterDbContext")))
            .ToList();

        violations.Should().BeEmpty(
            because: "controllers must delegate data access to service interfaces, not inject DbContext directly. Violations: {0}",
            string.Join("; ", violations));
    }

    [Fact]
    public void AllMutatingEndpointsUseDTOs()
    {
        var violations = GetRefactoredControllerTypes()
            .SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsMutatingActionMethod)
                .SelectMany(method => GetBodyParameters(method)
                    .Where(parameter => IsEntityModelType(parameter.ParameterType))
                    .Select(parameter =>
                        $"{controller.Name}.{method.Name} accepts entity model type '{parameter.ParameterType.Name}' instead of a DTO")))
            .ToList();

        violations.Should().BeEmpty(
            because: "POST/PUT endpoints on refactored controllers must accept DTO types. Violations: {0}",
            string.Join("; ", violations));
    }

    [Fact]
    public void MutatingEndpointsDoNotAcceptFromQueryEntityModels()
    {
        var violations = GetRefactoredControllerTypes()
            .SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsMutatingActionMethod)
                .SelectMany(method => method.GetParameters()
                    .Where(parameter => parameter.GetCustomAttribute<FromQueryAttribute>() != null)
                    .Where(parameter => IsEntityModelType(parameter.ParameterType))
                    .Select(parameter =>
                        $"{controller.Name}.{method.Name} accepts [FromQuery] entity model type '{parameter.ParameterType.Name}'")))
            .ToList();

        violations.Should().BeEmpty(
            because: "[FromQuery] parameters on mutating endpoints must not use entity model types. Violations: {0}",
            string.Join("; ", violations));
    }

    [Fact]
    public void AllServiceInterfacesRegistered()
    {
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        foreach (var serviceInterface in RequiredServiceInterfaces)
        {
            var service = serviceProvider.GetService(serviceInterface);
            service.Should().NotBeNull($"{serviceInterface.Name} must be registered in the DI container");
            service!.GetType().Should().Implement(serviceInterface);
        }
    }

    [Fact]
    public void RefactoredDtoValidatorsAreAutoDiscovered()
    {
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        foreach (var (dtoType, validatorType) in RefactoredDtoValidators)
        {
            var validatorInterface = typeof(IValidator<>).MakeGenericType(dtoType);
            var validator = serviceProvider.GetService(validatorInterface);

            validator.Should().NotBeNull(
                $"{validatorType.Name} must be registered for {dtoType.Name} via AddValidatorsFromAssemblyContaining");
            validator!.GetType().Should().Be(validatorType);
        }
    }

    private static IEnumerable<Type> GetControllerTypes() =>
        ApiAssembly.GetTypes()
            .Where(type => type.IsClass
                           && !type.IsAbstract
                           && typeof(ControllerBase).IsAssignableFrom(type)
                           && type.Namespace == typeof(AppointmentStatusController).Namespace);

    private static IEnumerable<Type> GetRefactoredControllerTypes() =>
        GetControllerTypes().Where(type => RefactoredControllerNames.Contains(type.Name));

    private static bool IsMutatingActionMethod(MethodInfo method)
    {
        if (method.GetCustomAttribute<NonActionAttribute>() != null)
        {
            return false;
        }

        return method.GetCustomAttribute<HttpPostAttribute>() != null
               || method.GetCustomAttribute<HttpPutAttribute>() != null;
    }

    private static IEnumerable<ParameterInfo> GetBodyParameters(MethodInfo method) =>
        method.GetParameters().Where(parameter =>
            parameter.GetCustomAttribute<FromQueryAttribute>() == null
            && parameter.GetCustomAttribute<FromRouteAttribute>() == null
            && parameter.GetCustomAttribute<FromHeaderAttribute>() == null
            && parameter.GetCustomAttribute<FromServicesAttribute>() == null
            && parameter.ParameterType != typeof(CancellationToken)
            && parameter.ParameterType != typeof(ClaimsPrincipal)
            && !typeof(ClaimsPrincipal).IsAssignableFrom(parameter.ParameterType)
            && !IsSimpleRouteParameter(parameter));

    private static bool IsSimpleRouteParameter(ParameterInfo parameter) =>
        parameter.ParameterType.IsPrimitive
        || parameter.ParameterType == typeof(string)
        || parameter.ParameterType == typeof(Guid)
        || parameter.ParameterType == typeof(decimal)
        || parameter.ParameterType == typeof(DateTime)
        || parameter.ParameterType == typeof(DateTimeOffset);

    private static bool IsEntityModelType(Type type)
    {
        if (type.Namespace != typeof(AppointmentStatus).Namespace)
        {
            return false;
        }

        return type.IsClass && type != typeof(MedicalCenterDbContext);
    }
}
