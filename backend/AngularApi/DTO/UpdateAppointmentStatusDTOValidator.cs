using AngularApi.Contracts.Enums;
using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.DTO
{
    public class UpdateAppointmentStatusDTOValidator : AbstractValidator<UpdateAppointmentStatusDTO>
    {
        public UpdateAppointmentStatusDTOValidator()
        {
            RuleFor(x => x.Status)
                .NotNull()
                .WithMessage("Status is required.")
                .IsInEnum()
                .WithMessage("Status must be a valid appointment status value.");
        }
    }
}
