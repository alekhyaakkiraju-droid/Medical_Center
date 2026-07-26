using AngularApi.DTO; using AngularApi.Validators; using FluentValidation.TestHelper; namespace AngularApi.Tests.DTO;
public class MedicalCenterDoctorAvailabilityDTOValidatorTests {
  static CreateMedicalCenterDoctorAvailabilityDTO C()=>new(){MedicalCenterId=1,DayOfWeek="Monday",StartTime=DateTime.Today.AddHours(9),EndTime=DateTime.Today.AddHours(17),IsAvailable=true};
  static UpdateMedicalCenterDoctorAvailabilityDTO U()=>new(){MedicalCenterId=1,DayOfWeek="Tuesday",StartTime=DateTime.Today.AddHours(10),EndTime=DateTime.Today.AddHours(18),IsAvailable=false,ReasonOfUnavailability="Holiday"};
  [Fact] public void CreateValid()=>new CreateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(C()).ShouldNotHaveAnyValidationErrors();
  [Fact] public void CreateBadCenter(){var d=C();d.MedicalCenterId=0;new CreateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.MedicalCenterId);}
  [Fact] public void CreateBadDay(){var d=C();d.DayOfWeek="Bad";new CreateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.DayOfWeek);}
  [Fact] public void CreateBadTime(){var d=C();d.EndTime=d.StartTime.AddHours(-1);new CreateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.EndTime);}
  [Fact] public void UpdateValid()=>new UpdateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(U()).ShouldNotHaveAnyValidationErrors();
  [Fact] public void UpdateEmptyDay(){var d=U();d.DayOfWeek="";new UpdateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.DayOfWeek);}
  [Fact] public void UpdateBadCenter(){var d=U();d.MedicalCenterId=-1;new UpdateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.MedicalCenterId);}
  [Fact] public void UpdateBadTime(){var d=U();d.EndTime=d.StartTime;new UpdateMedicalCenterDoctorAvailabilityDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.EndTime);}
}