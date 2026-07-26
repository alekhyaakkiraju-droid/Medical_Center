using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AngularApi.Infrastructure;

public static class DatabaseMigrationStartup
{
    public static Action<int> ExitApplication { get; set; } = Environment.Exit;

    public static async Task ApplyPendingMigrationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationRunner>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await migrator.ApplyPendingMigrationsAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to apply database migrations during startup.");
            ExitApplication(1);
        }
    }
}
