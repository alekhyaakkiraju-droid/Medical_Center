using AngularApi.Services.Interfaces;

namespace AngularApi.Middleware
{
    public class AuditMiddleware
    {
        private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Post,
            HttpMethods.Put,
            HttpMethods.Delete,
            HttpMethods.Patch
        };

        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            if (!MutatingMethods.Contains(context.Request.Method)
                || ShouldSkipPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();
            string requestBody;
            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            await _next(context);

            if (context.Response.StatusCode is >= 200 and < 300)
            {
                var entityType = ExtractEntityType(context.Request.Path);
                await auditService.RecordAsync(
                    context.Request.Method,
                    entityType,
                    entityId: null,
                    oldValues: null,
                    newValues: string.IsNullOrWhiteSpace(requestBody) ? null : requestBody);
            }
        }

        private static bool ShouldSkipPath(PathString path)
        {
            var value = path.Value ?? string.Empty;
            return value.StartsWith("/api/Account", StringComparison.OrdinalIgnoreCase);
        }

        private static string? ExtractEntityType(PathString path)
        {
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments == null || segments.Length < 2)
            {
                return null;
            }

            return segments[1].TrimEnd('s');
        }
    }
}
