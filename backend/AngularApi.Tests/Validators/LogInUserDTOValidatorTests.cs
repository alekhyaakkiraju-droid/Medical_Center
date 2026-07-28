using AngularApi.Contracts.DTO;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class LogInUserDTOValidatorTests
{
    private readonly LogInUserDTOValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_Passes()
    {
        var dto = new LogInUserDTO { Email = "user@example.com", Password = "secret1" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var dto = new LogInUserDTO { Email = "bad-email", Password = "secret1" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_MissingPassword_Fails()
    {
        var dto = new LogInUserDTO { Email = "user@example.com", Password = "" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
