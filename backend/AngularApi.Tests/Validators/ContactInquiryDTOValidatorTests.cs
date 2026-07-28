using AngularApi.Contracts.DTO;
using AngularApi.Tests.TestData;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class ContactInquiryDTOValidatorTests
{
    private readonly ContactInquiryDTOValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_Passes() =>
        _validator.TestValidate(ContactInquiryFixtures.Valid).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_ValidDtoWithoutPhone_Passes() =>
        _validator.TestValidate(ContactInquiryFixtures.ValidWithoutPhone).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_MissingName_Fails() =>
        _validator.TestValidate(ContactInquiryFixtures.MissingName).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Validate_InvalidEmail_Fails() =>
        _validator.TestValidate(ContactInquiryFixtures.InvalidEmail).ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Validate_InvalidName_Fails() =>
        _validator.TestValidate(ContactInquiryFixtures.InvalidName).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Validate_InvalidPhone_Fails() =>
        _validator.TestValidate(ContactInquiryFixtures.InvalidPhone).ShouldHaveValidationErrorFor(x => x.Phone);

    [Fact]
    public void Validate_MessageTooLong_Fails() =>
        _validator.TestValidate(ContactInquiryFixtures.MessageTooLong).ShouldHaveValidationErrorFor(x => x.Message);

    [Fact]
    public void Validate_MissingRecaptchaToken_Fails() =>
        _validator.TestValidate(ContactInquiryFixtures.MissingRecaptchaToken)
            .ShouldHaveValidationErrorFor(x => x.RecaptchaToken)
            .WithErrorMessage("reCAPTCHA token is required");
}
