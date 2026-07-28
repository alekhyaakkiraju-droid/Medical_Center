using AngularApi.Filters;
using AngularApi.Infrastructure;
using AngularApi.Contracts.Enums;
using AngularApi.Logging;
using AngularApi.Middleware;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace WebApiDemo
{
    public class Program
    {
        internal static void ApplyRecaptchaEnvironmentOverrides(IConfigurationBuilder configurationBuilder)
        {
            var recaptchaSiteKey = Environment.GetEnvironmentVariable("RECAPTCHA_SITE_KEY");
            var recaptchaSecretKey = Environment.GetEnvironmentVariable("RECAPTCHA_SECRET_KEY");

            if (string.IsNullOrWhiteSpace(recaptchaSiteKey) && string.IsNullOrWhiteSpace(recaptchaSecretKey))
            {
                return;
            }

            var overrides = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(recaptchaSiteKey))
            {
                overrides["RecaptchaSettings:SiteKey"] = recaptchaSiteKey;
            }

            if (!string.IsNullOrWhiteSpace(recaptchaSecretKey))
            {
                overrides["RecaptchaSettings:SecretKey"] = recaptchaSecretKey;
            }

            configurationBuilder.AddInMemoryCollection(overrides);
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddDockerSecrets();
            ApplyRecaptchaEnvironmentOverrides(builder.Configuration);
            builder.ConfigureSerilog();

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "MedCenter.AntiForgery";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Path = "/api";
            });

            builder.Services.AddScoped<ValidateAntiforgeryForMutatingRequestsFilter>();
            builder.Services.AddScoped<OwnershipValidationFilter>();
            builder.Services.AddControllers(options =>
            {
                options.Filters.AddService<ValidateAntiforgeryForMutatingRequestsFilter>();
                options.Filters.AddService<OwnershipValidationFilter>();
            });
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserDTOValidator>();
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerServices();
            builder.Services.AddAuthRateLimiting();
            builder.Services.AddHealthChecks();
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("admin"));
                options.AddPolicy("DoctorPolicy", policy => policy.RequireRole("doctor"));
                options.AddPolicy("UserPolicy", policy => policy.RequireRole("user"));
                options.AddPolicy("DoctorOrAdminPolicy", policy => policy.RequireRole("doctor", "admin"));
                options.AddPolicy("UserOrAdminPolicy", policy => policy.RequireRole("user", "admin"));
            });

            var app = builder.Build();
            var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
            var googleAuthOptions = app.Services.GetRequiredService<IOptions<GoogleAuthOptions>>().Value;
            if (!googleAuthOptions.IsConfigured)
            {
                startupLogger.LogWarning("Google OAuth is not configured; social login endpoints will return 503.");
            }

            JwtSecretStartupValidation.Validate(
                app.Configuration,
                app.Services.GetRequiredService<ILogger<Program>>());
            await DatabaseMigrationStartup.ApplyPendingMigrationsAsync(app.Services);

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await roleManager.EnsureRolesCreatedAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                await DevelopmentDataSeeder.SeedAsync(app.Services);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
            }

            app.UseResponseCompression();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseRateLimiter();
            app.UseStaticFiles();
            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<AuditMiddleware>();
            app.MapControllers();
            app.MapHealthChecks("/health").AllowAnonymous();
            app.Run();
        }
    }
}

public partial class Program { }
