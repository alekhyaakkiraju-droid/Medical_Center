using AngularApi.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class BreachAssessmentDTOValidator : AbstractValidator<BreachAssessmentDTO>
    {
        private static readonly string[] AllowedSeverityLevels = ["Low", "Medium", "High", "Critical"];

        public BreachAssessmentDTOValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(x => x.AffectedEntityTypes)
                .NotEmpty()
                .Must(types => types.All(type => !string.IsNullOrWhiteSpace(type)))
                .WithMessage("Affected entity types must not contain empty values.");

            RuleFor(x => x.DiscoveryDate)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
                .WithMessage("Discovery date cannot be in the future.");

            RuleFor(x => x.SeverityLevel)
                .NotEmpty()
                .Must(level => AllowedSeverityLevels.Contains(level, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Severity level must be one of: Low, Medium, High, Critical.");

            RuleForEach(x => x.AffectedIndividualEmails)
                .EmailAddress()
                .When(x => x.AffectedIndividualEmails.Count > 0);
        }
    }
}
