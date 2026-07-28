using FluentAssertions;

namespace AngularApi.Tests.Architecture;

/// <summary>
/// Verifies WO-015 DTO extraction: pure DTOs live in AngularApi.Contracts; AngularApi/DTO
/// retains only FluentValidation validators and EF Core query utilities.
/// </summary>
public class DtoExtractionArchitectureTests
{
    private static readonly HashSet<string> AllowedAngularApiDtoFiles =
    [
        "CreateAppointmentStatusDTOValidator.cs",
        "UpdateAppointmentStatusDTOValidator.cs",
        "CreateSpecializationDTOValidator.cs",
        "UpdateSpecializationDTOValidator.cs",
        "QueryProjections.cs",
        "QueryablePaginationExtensions.cs",
    ];

    [Fact]
    public void AngularApi_DtoFolder_ShouldOnlyContainValidatorsAndEfUtilities()
    {
        var dtoDirectory = Path.Combine(
            FindRepositoryRoot(),
            "backend",
            "AngularApi",
            "DTO");

        Directory.Exists(dtoDirectory).Should().BeTrue(because: "AngularApi/DTO directory must exist");

        var actualFiles = Directory
            .GetFiles(dtoDirectory, "*.cs")
            .Select(Path.GetFileName)
            .ToList();

        actualFiles.Should().BeEquivalentTo(AllowedAngularApiDtoFiles,
            because: "pure DTOs must be in AngularApi.Contracts/DTO; AngularApi/DTO keeps validators and EF helpers only");
    }

    [Fact]
    public void Contracts_DtoFolder_ShouldContainPureDtoTypes()
    {
        var contractsDtoDirectory = Path.Combine(
            FindRepositoryRoot(),
            "backend",
            "AngularApi.Contracts",
            "DTO");

        Directory.Exists(contractsDtoDirectory).Should().BeTrue();

        var dtoCount = Directory.GetFiles(contractsDtoDirectory, "*.cs").Length;
        dtoCount.Should().BeGreaterThanOrEqualTo(40,
            because: "WO-015 requires approximately 40 pure DTO files in AngularApi.Contracts/DTO");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "backend", "AngularApi.Contracts")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test output directory.");
    }
}
