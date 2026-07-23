using AngularApi.DTO;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class ForgotPasswordDTOValidatorTests
{
    private readonly ForgotPasswordDTOValidator _validator = new();

    [Fact]
    public void Validate_ValidEmail_Passes()
    {
        var dto = new ForgotPasswordDTO { Email = "user@example.com" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var dto = new ForgotPasswordDTO { Email = "bad-email" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
