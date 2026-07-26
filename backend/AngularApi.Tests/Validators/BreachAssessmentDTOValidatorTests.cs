using AngularApi.DTO;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class BreachAssessmentDTOValidatorTests
{
    private readonly BreachAssessmentDTOValidator _validator = new();

    private static BreachAssessmentDTO ValidAssessment() => new()
    {
        Description = "Unauthorized access to patient records",
        AffectedEntityTypes = ["Patient"],
        DiscoveryDate = DateTime.UtcNow.AddHours(-2),
        SeverityLevel = "High",
        AffectedIndividualEmails = ["patient@example.com"]
    };

    [Fact]
    public void Validate_ValidDto_Passes() =>
        _validator.TestValidate(ValidAssessment()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_MissingDescription_Fails() =>
        _validator.TestValidate(new BreachAssessmentDTO
        {
            Description = "",
            AffectedEntityTypes = ["Patient"],
            DiscoveryDate = DateTime.UtcNow.AddHours(-1),
            SeverityLevel = "High"
        }).ShouldHaveValidationErrorFor(x => x.Description);

    [Fact]
    public void Validate_EmptyAffectedEntityTypes_Fails() =>
        _validator.TestValidate(new BreachAssessmentDTO
        {
            Description = "Test breach",
            AffectedEntityTypes = [],
            DiscoveryDate = DateTime.UtcNow.AddHours(-1),
            SeverityLevel = "Medium"
        }).ShouldHaveValidationErrorFor(x => x.AffectedEntityTypes);

    [Fact]
    public void Validate_FutureDiscoveryDate_Fails() =>
        _validator.TestValidate(new BreachAssessmentDTO
        {
            Description = "Test breach",
            AffectedEntityTypes = ["Patient"],
            DiscoveryDate = DateTime.UtcNow.AddDays(1),
            SeverityLevel = "Low"
        }).ShouldHaveValidationErrorFor(x => x.DiscoveryDate);

    [Fact]
    public void Validate_InvalidSeverityLevel_Fails() =>
        _validator.TestValidate(new BreachAssessmentDTO
        {
            Description = "Test breach",
            AffectedEntityTypes = ["Patient"],
            DiscoveryDate = DateTime.UtcNow.AddHours(-1),
            SeverityLevel = "Urgent"
        }).ShouldHaveValidationErrorFor(x => x.SeverityLevel);

    [Fact]
    public void Validate_InvalidEmail_Fails() =>
        _validator.TestValidate(new BreachAssessmentDTO
        {
            Description = "Test breach",
            AffectedEntityTypes = ["Patient"],
            DiscoveryDate = DateTime.UtcNow.AddHours(-1),
            SeverityLevel = "Critical",
            AffectedIndividualEmails = ["not-an-email"]
        }).ShouldHaveValidationErrorFor("AffectedIndividualEmails[0]");
}
