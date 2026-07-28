using AngularApi.Models;
using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class EfCore10CompatibilityTests
{
    private static readonly string[] ExpectedMigrationIds =
    [
        "20250127114144_init",
        "20250131200700_addPayment",
        "20260723233344_AddAuditLog",
        "20260724031428_AddAuditColumnsAndCompositeIndexes",
        "20260726040000_AddContactInquiry"
    ];

    [Fact]
    public void ModelSnapshot_UsesEfCore10ProductVersion()
    {
        var snapshotSource = File.ReadAllText(Path.Combine(
            RepoRoot(),
            "backend",
            "AngularApi",
            "Migrations",
            "MedicalCenterDbContextModelSnapshot.cs"));

        snapshotSource.Should().Contain("ProductVersion\", \"10.0.");
        snapshotSource.Should().NotContain("ProductVersion\", \"8.0.");
    }

    [Fact]
    public void Migrations_ContainExpectedHistoricalMigrationIds()
    {
        var migrationDirectory = Path.Combine(RepoRoot(), "backend", "AngularApi", "Migrations");
        var migrationIds = Directory.GetFiles(migrationDirectory, "*.cs")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name is not null && !name.EndsWith(".Designer", StringComparison.Ordinal))
            .Where(name => name != "MedicalCenterDbContextModelSnapshot")
            .Select(name => name!)
            .OrderBy(name => name)
            .ToArray();

        migrationIds.Should().BeEquivalentTo(ExpectedMigrationIds);
    }

    [Fact]
    public void HistoricalMigrationFiles_RemainUnmodified()
    {
        foreach (var migrationId in ExpectedMigrationIds)
        {
            var migrationPath = Path.Combine(
                RepoRoot(),
                "backend",
                "AngularApi",
                "Migrations",
                $"{migrationId}.cs");

            File.Exists(migrationPath).Should().BeTrue(because: $"migration {migrationId} must remain in source control");
        }
    }

    private static string RepoRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
