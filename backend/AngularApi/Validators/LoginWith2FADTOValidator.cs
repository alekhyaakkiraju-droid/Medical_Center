using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class LoginWith2FADTOValidator : AbstractValidator<LoginWith2FADTO>
    {
        public LoginWith2FADTOValidator()
        {
            RuleFor(x => x.TwoFactorCode)
                .NotEmpty();
        }
    }
}
