using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class ReviewDTOValidator : AbstractValidator<ReviewDTO>
    {
        public ReviewDTOValidator()
        {
            RuleFor(x => x.WaitTimeRating)
                .InclusiveBetween(1, 5)
                .When(x => x.WaitTimeRating.HasValue);

            RuleFor(x => x.BedsideMannerRating)
                .InclusiveBetween(1, 5)
                .When(x => x.BedsideMannerRating.HasValue);

            RuleFor(x => x.OverallRating)
                .InclusiveBetween(1, 5)
                .When(x => x.OverallRating.HasValue);
        }
    }
}
