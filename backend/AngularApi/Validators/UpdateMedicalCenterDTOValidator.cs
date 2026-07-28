using AngularApi.Contracts.DTO;
using FluentValidation;
namespace AngularApi.Validators {
    public class UpdateMedicalCenterDTOValidator : AbstractValidator<UpdateMedicalCenterDTO> {
        public UpdateMedicalCenterDTOValidator() {             RuleFor(x => x.HospitalAffiliationId).GreaterThan(0).When(x => x.HospitalAffiliationId.HasValue);
            RuleFor(x => x.TimeSlotPerClientInMin).GreaterThan(0).When(x => x.TimeSlotPerClientInMin.HasValue);
            RuleFor(x => x.FirstConsultationFee).GreaterThan(0).When(x => x.FirstConsultationFee.HasValue);
            RuleFor(x => x.FollowupConsultationFee).GreaterThan(0).When(x => x.FollowupConsultationFee.HasValue);
            RuleFor(x => x.StreetAddress).NotEmpty().MaximumLength(200);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.State).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Zip).NotEmpty().MaximumLength(20); }
    }
}