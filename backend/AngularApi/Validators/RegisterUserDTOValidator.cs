using AngularApi.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class RegisterUserDTOValidator : AbstractValidator<RegisterUserDTO>
    {
        public RegisterUserDTOValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .Matches(ValidationConstants.AlphaNamePattern)
                .WithMessage("UserName must contain letters only.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .Must((dto, password) => !string.Equals(password, dto.Email, StringComparison.OrdinalIgnoreCase))
                .WithMessage("Password must not be the same as email.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("ConfirmPassword must match Password.");
        }
    }
}
