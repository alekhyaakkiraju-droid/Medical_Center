using AngularApi.Infrastructure;
using AngularApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace AngularApi.Tests.Infrastructure;

public class MedicalCenterWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"MedicalCenterTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:connection"] = "Server=(localdb)\\mssqllocaldb;Database=MedicalCenterTests;Trusted_Connection=True;",
                ["ConnectionStrings:SaPassword"] = "TestPassword123!",
                ["Jwt:ValidIssuer"] = "test-issuer",
                ["Jwt:ValidAudience"] = "test-audience",
                ["Jwt:Secret"] = "ThisIsAVeryLongSecretKeyForTestingPurposes123!",
                ["Jwt:AuthCookieName"] = "MedCenter.Auth",
                ["Jwt:RefreshCookieName"] = "MedCenter.Refresh",
                ["Jwt:CookiePath"] = "/api",
                ["Jwt:FrontendBaseUrl"] = "http://localhost:8081",
                ["GoogleAuth:ClientId"] = "test-client-id",
                ["GoogleAuth:ClientSecret"] = "test-client-secret",
                ["CorsSettings:AllowedOrigins:0"] = "http://localhost:4200",
                ["CorsSettings:AllowedOrigins:1"] = "http://localhost:8081",
                ["RecaptchaSettings:Enabled"] = "false",
            });
        });

        builder.ConfigureServices(services =>
        {
            RemoveDbContextRegistrations(services);

            services.AddDbContext<MedicalCenterDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            ReplaceDatabaseMigrationRunner(services);
            services.AddSingleton<IStartupFilter, TestClientIpStartupFilter>();
        });
    }

    private static void RemoveDbContextRegistrations(IServiceCollection services)
    {
        var descriptors = services
            .Where(d =>
                d.ServiceType == typeof(MedicalCenterDbContext)
                || d.ServiceType == typeof(DbContextOptions<MedicalCenterDbContext>)
                || d.ServiceType == typeof(IDbContextOptionsConfiguration<MedicalCenterDbContext>))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void ReplaceDatabaseMigrationRunner(IServiceCollection services)
    {
        var migrationRunnerDescriptors = services
            .Where(d => d.ServiceType == typeof(IDatabaseMigrationRunner))
            .ToList();

        foreach (var descriptor in migrationRunnerDescriptors)
        {
            services.Remove(descriptor);
        }

        services.AddScoped<IDatabaseMigrationRunner, NoOpDatabaseMigrationRunner>();
    }

    private sealed class NoOpDatabaseMigrationRunner : IDatabaseMigrationRunner
    {
        public Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class TestClientIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    if (context.Request.Headers.TryGetValue("X-Test-Client-Ip", out var testIp)
                        && IPAddress.TryParse(testIp!, out var ipAddress))
                    {
                        context.Connection.RemoteIpAddress = ipAddress;
                    }

                    await nextMiddleware();
                });

                next(app);
            };
        }
    }
}
