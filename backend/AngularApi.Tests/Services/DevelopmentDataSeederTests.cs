using AngularApi.Models;
using AngularApi.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Services;

public class DevelopmentDataSeederTests
{
    private static ServiceProvider CreateProvider()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(_ => new MedicalCenterDbContext(options));

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedAsync_AddsSpecializationsWhenDatabaseIsEmpty()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var count = await context.Specializations.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotentWhenSpecializationsExist()
    {
        await using var provider = CreateProvider();

        await DevelopmentDataSeeder.SeedAsync(provider);
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var count = await context.Specializations.CountAsync();
        count.Should().Be(3);
    }
}
