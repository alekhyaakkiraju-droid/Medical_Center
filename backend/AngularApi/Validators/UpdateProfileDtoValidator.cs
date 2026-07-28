using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(ValidationConstants.PhonePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("PhoneNumber must be 10 to 15 digits.");
        }
    }
}
