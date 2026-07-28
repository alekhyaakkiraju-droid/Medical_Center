using AngularApi.Contracts.DTO;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class UpdateProfileDtoValidatorTests
{
    private readonly UpdateProfileDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidOptionalFields_Passes()
    {
        var dto = new UpdateProfileDto
        {
            Email = "user@example.com",
            PhoneNumber = "1234567890"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var dto = new UpdateProfileDto { Email = "invalid-email" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidPhoneNumber_Fails()
    {
        var dto = new UpdateProfileDto { PhoneNumber = "abc" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
