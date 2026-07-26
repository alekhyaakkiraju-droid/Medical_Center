using AngularApi.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AngularApi.Tests.Infrastructure;

public class EfCore10CompatibilityTests
{
    [Fact]
    public void ModelSnapshot_UsesEfCore10ProductVersion()
    {
        var snapshotSource = File.ReadAllText(Path.Combine(RepoRoot(), "backend", "AngularApi", "Migrations", "MedicalCenterDbContextModelSnapshot.cs"));
        snapshotSource.Should().Contain("ProductVersion\", \"10.0.");
    }

    [Fact]
    public void ModelSnapshot_HasNoPendingSchemaChanges()
    {
        using var context = CreateContext();
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        var modelDiffer = context.GetService<IMigrationsModelDiffer>();
        modelDiffer.HasDifferences(designTimeModel, context.Model).Should().BeFalse();
    }

    private static MedicalCenterDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new MedicalCenterDbContext(options);
    }

    private static string RepoRoot() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
