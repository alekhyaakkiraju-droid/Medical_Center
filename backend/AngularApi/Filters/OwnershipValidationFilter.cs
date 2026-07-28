using AngularApi.Middleware;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Filters;

public class OwnershipValidationFilter : IAsyncActionFilter
{
    private readonly IOwnershipValidator _ownershipValidator;
    private readonly MedicalCenterDbContext _dbContext;
    private readonly ILogger<OwnershipValidationFilter> _logger;

    public OwnershipValidationFilter(
        IOwnershipValidator ownershipValidator,
        MedicalCenterDbContext dbContext,
        ILogger<OwnershipValidationFilter> logger)
    {
        _ownershipValidator = ownershipValidator;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attributes = context.ActionDescriptor.EndpointMetadata
            .OfType<ValidateOwnershipAttribute>()
            .ToList();

        if (attributes.Count == 0)
        {
            await next();
            return;
        }

        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        foreach (var attribute in attributes)
        {
            if (!await ValidateOwnershipAsync(context, user, attribute))
            {
                var actorId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.HeaderName]?.ToString();
                var resourceId = TryGetResourceId(context, attribute.IdParameterName);

                _logger.LogWarning(
                    "Ownership validation denied for {ResourceType} resource {ResourceId} on action {Action}. ActorId={ActorId}, CorrelationId={CorrelationId}",
                    attribute.ResourceType,
                    resourceId,
                    context.ActionDescriptor.DisplayName,
                    actorId,
                    correlationId);

                context.Result = new ObjectResult(new
                {
                    error = "Forbidden",
                    message = "You do not have permission to access this resource",
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                };

                return;
            }
        }

        await next();
    }

    private async Task<bool> ValidateOwnershipAsync(
        ActionExecutingContext context,
        System.Security.Claims.ClaimsPrincipal user,
        ValidateOwnershipAttribute attribute)
    {
        return attribute.ResourceType switch
        {
            ResourceType.Patient => ValidatePatientOwnership(context, user, attribute),
            ResourceType.Doctor => ValidateDoctorOwnership(context, user, attribute),
            ResourceType.PatientReview => await ValidatePatientReviewOwnershipAsync(context, user, attribute),
            ResourceType.MedicalCenter => _ownershipValidator.CanAccessMedicalCenterResource(user),
            ResourceType.Appointment => await ValidateAppointmentOwnershipAsync(context, user, attribute),
            _ => true,
        };
    }

    private bool ValidatePatientOwnership(
        ActionExecutingContext context,
        System.Security.Claims.ClaimsPrincipal user,
        ValidateOwnershipAttribute attribute)
    {
        var patientId = TryGetResourceId(context, attribute.IdParameterName);
        if (patientId == null)
        {
            LogMissingRouteParameter(context, attribute);
            return true;
        }

        return _ownershipValidator.CanAccessPatientResource(user, patientId);
    }

    private bool ValidateDoctorOwnership(
        ActionExecutingContext context,
        System.Security.Claims.ClaimsPrincipal user,
        ValidateOwnershipAttribute attribute)
    {
        var doctorId = TryGetResourceId(context, attribute.IdParameterName);
        if (doctorId == null)
        {
            LogMissingRouteParameter(context, attribute);
            return true;
        }

        return _ownershipValidator.CanAccessDoctorResource(user, doctorId);
    }

    private async Task<bool> ValidateAppointmentOwnershipAsync(
        ActionExecutingContext context,
        System.Security.Claims.ClaimsPrincipal user,
        ValidateOwnershipAttribute attribute)
    {
        var appointmentIdValue = TryGetResourceId(context, attribute.IdParameterName);
        if (appointmentIdValue == null || !int.TryParse(appointmentIdValue, out var appointmentId))
        {
            _logger.LogWarning(
                "Ownership validation denied for action {Action}: route parameter '{ParameterName}' was missing or invalid for resource type {ResourceType}.",
                context.ActionDescriptor.DisplayName,
                attribute.IdParameterName,
                attribute.ResourceType);

            return false;
        }

        return await _ownershipValidator.CanAccessAppointmentResource(user, appointmentId);
    }

    private async Task<bool> ValidatePatientReviewOwnershipAsync(
        ActionExecutingContext context,
        System.Security.Claims.ClaimsPrincipal user,
        ValidateOwnershipAttribute attribute)
    {
        var reviewIdValue = TryGetResourceId(context, attribute.IdParameterName);
        if (reviewIdValue == null || !int.TryParse(reviewIdValue, out var reviewId))
        {
            LogMissingRouteParameter(context, attribute);
            return true;
        }

        var reviewPatientId = await _dbContext.PatientReviews
            .AsNoTracking()
            .Where(review => review.Id == reviewId)
            .Select(review => review.PatientId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(reviewPatientId))
        {
            return true;
        }

        return _ownershipValidator.CanAccessPatientReviewResource(user, reviewPatientId);
    }

    private void LogMissingRouteParameter(ActionExecutingContext context, ValidateOwnershipAttribute attribute)
    {
        _logger.LogWarning(
            "Ownership validation skipped for action {Action}: route parameter '{ParameterName}' was not found for resource type {ResourceType}.",
            context.ActionDescriptor.DisplayName,
            attribute.IdParameterName,
            attribute.ResourceType);
    }

    private static string? TryGetResourceId(ActionExecutingContext context, string idParameterName)
    {
        if (context.ActionArguments.TryGetValue(idParameterName, out var argumentValue) && argumentValue != null)
        {
            return Convert.ToString(argumentValue, System.Globalization.CultureInfo.InvariantCulture);
        }

        if (context.RouteData.Values.TryGetValue(idParameterName, out var routeValue) && routeValue != null)
        {
            return Convert.ToString(routeValue, System.Globalization.CultureInfo.InvariantCulture);
        }

        return null;
    }
}
