using AngularApi.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class LogInUserDTOValidator : AbstractValidator<LogInUserDTO>
    {
        public LogInUserDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
