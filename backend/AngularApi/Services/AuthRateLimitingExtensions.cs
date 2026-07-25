using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AngularApi.Services;

public static class AuthRateLimitingExtensions
{
    public const string LoginPolicy = "auth-login";
    public const string RegisterPolicy = "auth-register";
    public const string ForgotPasswordPolicy = "auth-forgot-password";

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(LoginPolicy, context => CreateFixedWindowLimiter(context, 5));
            options.AddPolicy(RegisterPolicy, context => CreateFixedWindowLimiter(context, 3));
            options.AddPolicy(ForgotPasswordPolicy, context => CreateFixedWindowLimiter(context, 3));
        });

        return services;
    }

    private static RateLimitPartition<string> CreateFixedWindowLimiter(HttpContext context, int permitLimit)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            clientIp,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    }
}
