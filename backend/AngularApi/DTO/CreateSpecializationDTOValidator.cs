using FluentValidation;

namespace AngularApi.DTO
{
    public class CreateSpecializationDTOValidator : AbstractValidator<CreateSpecializationDTO>
    {
        public CreateSpecializationDTOValidator()
        {
            RuleFor(x => x.SpecializationName)
                .NotEmpty()
                .WithMessage("SpecializationName is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => x.Description != null);
        }
    }
}
