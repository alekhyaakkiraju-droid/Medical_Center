namespace AngularApi.Infrastructure;

public interface IDatabaseMigrationRunner
{
    Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default);
}
