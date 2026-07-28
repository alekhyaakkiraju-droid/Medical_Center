using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AngularApi.Filters;

public class NoCachePhiActionFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var requiresNoCache = endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null
            && endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null;

        if (requiresNoCache)
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            context.HttpContext.Response.Headers.Pragma = "no-cache";
        }

        await next();
    }
}
