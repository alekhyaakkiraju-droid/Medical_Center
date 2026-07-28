using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.DTO
{
    public class UpdateSpecializationDTOValidator : AbstractValidator<UpdateSpecializationDTO>
    {
        public UpdateSpecializationDTOValidator()
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
