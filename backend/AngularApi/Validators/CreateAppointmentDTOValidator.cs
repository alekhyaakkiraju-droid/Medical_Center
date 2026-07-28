using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.Validators;

public class CreateAppointmentDTOValidator : AbstractValidator<CreateAppointmentDTO>
{
    public CreateAppointmentDTOValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .WithMessage("DoctorId is required.");

        RuleFor(x => x.MedicalCenterId)
            .GreaterThan(0);

        RuleFor(x => x.AppointmentTakenDate)
            .Must(date => date.Date >= DateTime.UtcNow.Date)
            .WithMessage("AppointmentTakenDate must be today or a future date.");

        RuleFor(x => x.ProbableStartTime)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Phone)
            .NotEmpty();
    }
}
