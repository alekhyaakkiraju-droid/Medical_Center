using AngularApi.DTO;
using FluentValidation;

namespace AngularApi.Validators
{
    public class UpdateAppointmentDTOValidator : AbstractValidator<UpdateAppointmentDTO>
    {
        public UpdateAppointmentDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.AppointmentTakenDate)
                .NotNull()
                .Must(date => date!.Value.Date >= DateTime.UtcNow.Date)
                .WithMessage("AppointmentTakenDate must be today or a future date.");
        }
    }
}
