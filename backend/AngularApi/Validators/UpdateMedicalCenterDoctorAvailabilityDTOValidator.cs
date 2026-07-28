using AngularApi.Contracts.DTO;
using FluentValidation;
namespace AngularApi.Validators {
    public class UpdateMedicalCenterDoctorAvailabilityDTOValidator : AbstractValidator<UpdateMedicalCenterDoctorAvailabilityDTO> {
        private static readonly string[] ValidDaysOfWeek = ["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"];
        public UpdateMedicalCenterDoctorAvailabilityDTOValidator() {             RuleFor(x => x.MedicalCenterId).GreaterThan(0);
            RuleFor(x => x.DayOfWeek).NotEmpty().Must(day => ValidDaysOfWeek.Contains(day, StringComparer.OrdinalIgnoreCase)).WithMessage("DayOfWeek must be a valid day name (Sunday through Saturday).");
            RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime."); }
    }
}