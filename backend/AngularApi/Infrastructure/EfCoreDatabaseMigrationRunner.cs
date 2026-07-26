using AngularApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Infrastructure;

public sealed class EfCoreDatabaseMigrationRunner : IDatabaseMigrationRunner
{
    private readonly MedicalCenterDbContext _context;

    public EfCoreDatabaseMigrationRunner(MedicalCenterDbContext context)
    {
        _context = context;
    }

    public async Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.IsRelational())
        {
            await _context.Database.MigrateAsync(cancellationToken);
        }
    }
}
