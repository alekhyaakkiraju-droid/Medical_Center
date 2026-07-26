using AngularApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                ["Jwt:ValidIssuer"] = "test-issuer",
                ["Jwt:ValidAudience"] = "test-audience",
                ["Jwt:Secret"] = "ThisIsAVeryLongSecretKeyForTestingPurposes123!",
                ["Jwt:AuthCookieName"] = "MedCenter.Auth",
                ["Jwt:RefreshCookieName"] = "MedCenter.Refresh",
                ["Jwt:CookiePath"] = "/api",
                ["GoogleAuth:ClientId"] = "test-client-id",
                ["GoogleAuth:ClientSecret"] = "test-client-secret",
                ["CorsSettings:AllowedOrigins:0"] = "http://localhost:4200",
                ["CorsSettings:AllowedOrigins:1"] = "http://localhost:8081",
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MedicalCenterDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<MedicalCenterDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }
}
