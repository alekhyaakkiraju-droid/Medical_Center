using AngularApi.Contracts.Enums;
using AngularApi.Contracts.DTO;
using FluentValidation;

namespace AngularApi.DTO
{
    public class CreateAppointmentStatusDTOValidator : AbstractValidator<CreateAppointmentStatusDTO>
    {
        public CreateAppointmentStatusDTOValidator()
        {
            RuleFor(x => x.Status)
                .NotNull()
                .WithMessage("Status is required.")
                .IsInEnum()
                .WithMessage("Status must be a valid appointment status value.");
        }
    }
}
