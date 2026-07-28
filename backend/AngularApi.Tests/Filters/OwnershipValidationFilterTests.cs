using AngularApi.Models;
using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;
using AngularApi.Filters;
using AngularApi.Services.impelementation;
using AngularApi.Contracts.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace AngularApi.Tests.Filters;

public class OwnershipValidationFilterTests
{
    [Fact]
    public async Task OnActionExecutionAsync_WithoutValidateOwnershipAttribute_AllowsRequest()
    {
        await using var dbContext = CreateDbContext();
        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(userId: "patient-1", roles: ["user"]);
        var nextCalled = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_UnauthenticatedUser_SkipsValidation()
    {
        var ownershipValidator = new Mock<IOwnershipValidator>(MockBehavior.Strict);
        var filter = CreateFilter(ownershipValidator.Object, CreateDbContext());
        var context = CreateContext(
            userId: null,
            roles: [],
            attributes: [new ValidateOwnershipAttribute(ResourceType.MedicalCenter)]);
        var nextCalled = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_AdminUpdatingMedicalCenter_AllowsRequest()
    {
        await using var dbContext = CreateDbContext();
        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "admin-user",
            roles: ["admin"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.MedicalCenter)],
            routeValues: new Dictionary<string, object?> { ["id"] = 1 });

        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_NonAdminUpdatingMedicalCenter_ReturnsForbidden()
    {
        await using var dbContext = CreateDbContext();
        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "doctor-1",
            roles: ["doctor"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.MedicalCenter)],
            routeValues: new Dictionary<string, object?> { ["id"] = 1 });

        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        context.Result.Should().BeOfType<ObjectResult>();
        var result = (ObjectResult)context.Result!;
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task OnActionExecutionAsync_PatientUpdatingOwnReview_AllowsRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.PatientReviews.Add(new PatientReview { Id = 7, PatientId = "patient-1", OverallRating = 4 });
        await dbContext.SaveChangesAsync();

        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "patient-1",
            roles: ["user"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.PatientReview)],
            actionArguments: new Dictionary<string, object?> { ["id"] = 7 });

        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_PatientUpdatingAnotherReview_ReturnsForbidden()
    {
        await using var dbContext = CreateDbContext();
        dbContext.PatientReviews.Add(new PatientReview { Id = 8, PatientId = "patient-2", OverallRating = 4 });
        await dbContext.SaveChangesAsync();

        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "patient-1",
            roles: ["user"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.PatientReview)],
            actionArguments: new Dictionary<string, object?> { ["id"] = 8 });

        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        context.Result.Should().BeOfType<ObjectResult>();
        var result = (ObjectResult)context.Result!;
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        result.Value.Should().BeEquivalentTo(new
        {
            error = "Forbidden",
            message = "You do not have permission to access this resource",
        });
    }

    [Fact]
    public async Task OnActionExecutionAsync_MissingRouteParameter_SkipsValidation()
    {
        var ownershipValidator = new Mock<IOwnershipValidator>(MockBehavior.Strict);
        var filter = CreateFilter(ownershipValidator.Object, CreateDbContext());
        var context = CreateContext(
            userId: "patient-1",
            roles: ["user"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.Patient, "patientId")]);

        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_AdminUpdatingAnyReview_AllowsRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.PatientReviews.Add(new PatientReview { Id = 9, PatientId = "patient-2", OverallRating = 3 });
        await dbContext.SaveChangesAsync();

        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "admin-user",
            roles: ["admin"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.PatientReview)],
            actionArguments: new Dictionary<string, object?> { ["id"] = 9 });

        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_PatientAccessingOwnAppointment_AllowsRequest()
    {
        await using var dbContext = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(dbContext, "patient-1", "doctor-1");

        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "patient-1",
            roles: ["user"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.Appointment)],
            actionArguments: new Dictionary<string, object?> { ["id"] = appointmentId });

        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_DoctorAccessingAssignedAppointment_AllowsRequest()
    {
        await using var dbContext = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(dbContext, "patient-1", "doctor-1");

        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "doctor-1",
            roles: ["doctor"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.Appointment)],
            actionArguments: new Dictionary<string, object?> { ["id"] = appointmentId });

        var nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_NonOwnerAccessingAppointment_ReturnsForbidden()
    {
        await using var dbContext = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(dbContext, "patient-1", "doctor-1");

        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "patient-2",
            roles: ["user"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.Appointment)],
            actionArguments: new Dictionary<string, object?> { ["id"] = appointmentId });

        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        context.Result.Should().BeOfType<ObjectResult>();
        var result = (ObjectResult)context.Result!;
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task OnActionExecutionAsync_InvalidAppointmentId_ReturnsForbidden()
    {
        await using var dbContext = CreateDbContext();
        var filter = CreateFilter(new OwnershipValidator(dbContext), dbContext);
        var context = CreateContext(
            userId: "patient-1",
            roles: ["user"],
            attributes: [new ValidateOwnershipAttribute(ResourceType.Appointment)],
            actionArguments: new Dictionary<string, object?> { ["id"] = "not-an-int" });

        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        context.Result.Should().BeOfType<ObjectResult>();
        var result = (ObjectResult)context.Result!;
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    private static async Task<int> SeedAppointmentAsync(
        MedicalCenterDbContext context,
        string patientId,
        string? doctorId)
    {
        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DoctorName = doctorId ?? "Unassigned",
            AppointmentTakenDate = DateTime.UtcNow,
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }

    private static OwnershipValidationFilter CreateFilter(
        IOwnershipValidator ownershipValidator,
        MedicalCenterDbContext dbContext)
    {
        var logger = new Mock<ILogger<OwnershipValidationFilter>>();
        return new OwnershipValidationFilter(ownershipValidator, dbContext, logger.Object);
    }

    private static MedicalCenterDbContext CreateDbContext()
    {
        return new MedicalCenterDbContext(
            new DbContextOptionsBuilder<MedicalCenterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
    }

    private static ActionExecutingContext CreateContext(
        string? userId,
        string[] roles,
        ValidateOwnershipAttribute[]? attributes = null,
        Dictionary<string, object?>? routeValues = null,
        Dictionary<string, object?>? actionArguments = null)
    {
        var httpContext = new DefaultHttpContext();
        if (userId != null)
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        var actionDescriptor = new ActionDescriptor
        {
            DisplayName = "TestAction",
            EndpointMetadata = attributes?.Cast<object>().ToList() ?? [],
        };

        var routeData = new RouteData();
        foreach (var entry in routeValues ?? [])
        {
            routeData.Values[entry.Key] = entry.Value;
        }

        return new ActionExecutingContext(
            new ActionContext(httpContext, routeData, actionDescriptor),
            [],
            actionArguments ?? [],
            controller: new object());
    }
}
