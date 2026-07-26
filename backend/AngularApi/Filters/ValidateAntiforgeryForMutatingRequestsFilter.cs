using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AngularApi.Filters;

public class ValidateAntiforgeryForMutatingRequestsFilter : IAsyncActionFilter
{
    private readonly IAntiforgery _antiforgery;

    public ValidateAntiforgeryForMutatingRequestsFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var requiresValidation = HttpMethods.IsPost(method)
            || HttpMethods.IsPut(method)
            || HttpMethods.IsDelete(method)
            || HttpMethods.IsPatch(method);

        if (requiresValidation
            && !context.ActionDescriptor.EndpointMetadata.Any(metadata => metadata is IgnoreAntiforgeryTokenAttribute))
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (AntiforgeryValidationException)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = "Antiforgery validation failed.",
                    message = "Mutating requests require a valid X-XSRF-TOKEN header.",
                });
                return;
            }
        }

        await next();
    }
}
