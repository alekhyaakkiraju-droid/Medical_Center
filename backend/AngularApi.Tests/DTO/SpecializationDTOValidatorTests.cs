using AngularApi.DTO;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.DTO;

public class SpecializationDTOValidatorTests
{
    private readonly CreateSpecializationDTOValidator _createValidator = new();
    private readonly UpdateSpecializationDTOValidator _updateValidator = new();

    [Fact]
    public void CreateValidator_ValidDto_Passes()
    {
        var dto = new CreateSpecializationDTO
        {
            SpecializationName = "Cardiology",
            Description = "Heart specialist",
            IsActive = true
        };
        _createValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_EmptySpecializationName_Fails()
    {
        var dto = new CreateSpecializationDTO { SpecializationName = string.Empty };
        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SpecializationName);
    }

    [Fact]
    public void CreateValidator_SpecializationNameTooLong_Fails()
    {
        var dto = new CreateSpecializationDTO { SpecializationName = new string('A', 101) };
        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SpecializationName);
    }

    [Fact]
    public void CreateValidator_DescriptionTooLong_Fails()
    {
        var dto = new CreateSpecializationDTO
        {
            SpecializationName = "Cardiology",
            Description = new string('D', 501)
        };
        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void UpdateValidator_ValidDto_Passes()
    {
        var dto = new UpdateSpecializationDTO
        {
            SpecializationName = "Neurology",
            Description = "Brain specialist",
            IsActive = false
        };
        _updateValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValidator_NullSpecializationName_Fails()
    {
        var dto = new UpdateSpecializationDTO { SpecializationName = null };
        _updateValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SpecializationName);
    }

    [Fact]
    public void UpdateValidator_SpecializationNameTooLong_Fails()
    {
        var dto = new UpdateSpecializationDTO { SpecializationName = new string('N', 101) };
        _updateValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SpecializationName);
    }

    [Fact]
    public void UpdateValidator_DescriptionTooLong_Fails()
    {
        var dto = new UpdateSpecializationDTO
        {
            SpecializationName = "Neurology",
            Description = new string('X', 501)
        };
        _updateValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Description);
    }
}
