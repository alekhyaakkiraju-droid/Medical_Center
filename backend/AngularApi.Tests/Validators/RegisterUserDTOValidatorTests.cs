using AngularApi.Contracts.DTO;
using AngularApi.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class RegisterUserDTOValidatorTests
{
    private readonly RegisterUserDTOValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_Passes()
    {
        var dto = new RegisterUserDTO
        {
            UserName = "Jane Doe",
            Email = "jane@example.com",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var dto = new RegisterUserDTO
        {
            UserName = "Jane Doe",
            Email = "not-an-email",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_PasswordSameAsEmail_Fails()
    {
        var dto = new RegisterUserDTO
        {
            UserName = "Jane Doe",
            Email = "secret1@example.com",
            Password = "secret1@example.com",
            ConfirmPassword = "secret1@example.com"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_NonAlphaUserName_Fails()
    {
        var dto = new RegisterUserDTO
        {
            UserName = "Jane123",
            Email = "jane@example.com",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }
}
