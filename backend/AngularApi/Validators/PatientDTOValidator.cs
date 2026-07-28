using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class PatientDTOValidator : AbstractValidator<PatientDTO>
    {
        public PatientDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Matches(ValidationConstants.AlphaNamePattern)
                .WithMessage("Name must contain letters only.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleForEach(x => x.Reviews)
                .SetValidator(new ReviewDTOValidator())
                .When(x => x.Reviews != null);
        }
    }
}
