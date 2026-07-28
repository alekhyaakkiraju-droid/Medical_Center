using AngularApi.Contracts.DTO;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class PatientDTOValidatorTests
{
    private readonly PatientDTOValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_Passes()
    {
        var dto = new PatientDTO
        {
            Name = "John Doe",
            Email = "john@example.com",
            Reviews = new List<ReviewDTO>
            {
                new() { OverallRating = 5 }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidReviewRating_Fails()
    {
        var dto = new PatientDTO
        {
            Name = "John Doe",
            Email = "john@example.com",
            Reviews = new List<ReviewDTO>
            {
                new() { OverallRating = 10 }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Reviews[0].OverallRating");
    }
}
