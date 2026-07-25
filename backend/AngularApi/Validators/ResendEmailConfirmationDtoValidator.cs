using AngularApi.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class ResendEmailConfirmationDtoValidator : AbstractValidator<ResendEmailConfirmationDto>
    {
        public ResendEmailConfirmationDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
