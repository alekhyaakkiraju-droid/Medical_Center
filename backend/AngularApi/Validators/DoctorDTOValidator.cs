using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class DoctorDTOValidator : AbstractValidator<DoctorDTO>
    {
        public DoctorDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .When(x => x != null);

            RuleFor(x => x.Name)
                .Matches(ValidationConstants.AlphaNamePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.Name))
                .WithMessage("Doctor name must contain letters only.");
        }
    }
}
