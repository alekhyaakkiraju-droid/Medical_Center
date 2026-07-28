using AngularApi.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class ContactInquiryDTOValidator : AbstractValidator<ContactInquiryDTO>
    {
        public ContactInquiryDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Matches(ValidationConstants.AlphaNamePattern)
                .WithMessage("Name must contain letters only.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Phone)
                .Matches(ValidationConstants.PhonePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Phone must contain 10 to 15 digits.");

            RuleFor(x => x.Message)
                .NotEmpty()
                .MaximumLength(ValidationConstants.ReviewMaxLength);

            RuleFor(x => x.RecaptchaToken)
                .NotEmpty()
                .WithMessage("reCAPTCHA token is required");
        }
    }
}
