using AngularApi.Infrastructure;
using AngularApi.Filters;
using AngularApi.Logging;
using AngularApi.Middleware;
using AngularApi.Services;
using AngularApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebApiDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddDockerSecrets();
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
            builder.Services.AddControllers(options =>
            {
                options.Filters.AddService<ValidateAntiforgeryForMutatingRequestsFilter>();
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
