using AngularApi.DTO;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class ResetPasswordDTOValidatorTests
{
    private readonly ResetPasswordDTOValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_Passes()
    {
        var dto = new ResetPasswordDTO
        {
            Email = "user@example.com",
            Token = "reset-token",
            NewPassword = "newpass1"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShortPassword_Fails()
    {
        var dto = new ResetPasswordDTO
        {
            Email = "user@example.com",
            Token = "reset-token",
            NewPassword = "123"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
