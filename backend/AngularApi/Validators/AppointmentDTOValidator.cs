using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class AppointmentDTOValidator : AbstractValidator<AppointmentDTO>
    {
        public AppointmentDTOValidator()
        {
            RuleFor(x => x.Doctor)
                .NotNull()
                .WithMessage("Doctor is required.");

            RuleFor(x => x.Doctor!.Name)
                .NotEmpty()
                .When(x => x.Doctor != null)
                .WithMessage("DoctorName is required.");

            RuleFor(x => x.AppointmentDate)
                .NotNull()
                .WithMessage("AppointmentDate is required.")
                .Must(date => date!.Value.Date > DateTime.UtcNow.Date)
                .WithMessage("AppointmentDate must be a future date.");
        }
    }
}
