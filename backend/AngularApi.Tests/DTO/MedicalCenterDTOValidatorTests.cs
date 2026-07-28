using AngularApi.Contracts.DTO; using AngularApi.Validators; using FluentValidation.TestHelper; namespace AngularApi.Tests.DTO;
public class MedicalCenterDTOValidatorTests {
  static CreateMedicalCenterDTO C()=>new(){StreetAddress="123 Main St",City="Boston",State="MA",Zip="02101",FirstConsultationFee=100m,FollowupConsultationFee=75m,TimeSlotPerClientInMin=30};
  static UpdateMedicalCenterDTO U()=>new(){StreetAddress="123 Main St",City="Boston",State="MA",Zip="02101",FirstConsultationFee=100m,FollowupConsultationFee=75m,TimeSlotPerClientInMin=30};
  [Fact] public void CreateValid()=>new CreateMedicalCenterDTOValidator().TestValidate(C()).ShouldNotHaveAnyValidationErrors();
  [Fact] public void CreateEmptyStreet(){var d=C();d.StreetAddress="";new CreateMedicalCenterDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.StreetAddress);}
  [Fact] public void CreateEmptyCity(){var d=C();d.City="";new CreateMedicalCenterDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.City);}
  [Fact] public void CreateBadFee(){var d=C();d.FirstConsultationFee=0m;new CreateMedicalCenterDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.FirstConsultationFee);}
  [Fact] public void UpdateValid()=>new UpdateMedicalCenterDTOValidator().TestValidate(U()).ShouldNotHaveAnyValidationErrors();
  [Fact] public void UpdateEmptyState(){var d=U();d.State="";new UpdateMedicalCenterDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.State);}
  [Fact] public void UpdateEmptyZip(){var d=U();d.Zip="";new UpdateMedicalCenterDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.Zip);}
  [Fact] public void UpdateBadSlot(){var d=U();d.TimeSlotPerClientInMin=0;new UpdateMedicalCenterDTOValidator().TestValidate(d).ShouldHaveValidationErrorFor(x=>x.TimeSlotPerClientInMin);}
}