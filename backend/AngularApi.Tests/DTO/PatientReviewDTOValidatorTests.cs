using AngularApi.Contracts.DTO; using AngularApi.Validators; using FluentValidation.TestHelper;
namespace AngularApi.Tests.DTO;
public class PatientReviewDTOValidatorTests {
  private readonly CreatePatientReviewDTOValidator _c=new(); private readonly UpdatePatientReviewDTOValidator _u=new();
  [Fact] public void CreateValidate_ValidDto_Passes()=>_c.TestValidate(new CreatePatientReviewDTO{DoctorId="d1",OverallRating=5,WaitTimeRating=4,BedsideMannerRating=5,Review="ok"}).ShouldNotHaveAnyValidationErrors();
  [Fact] public void CreateValidate_MissingDoctorId_Fails()=>_c.TestValidate(new CreatePatientReviewDTO{OverallRating=5}).ShouldHaveValidationErrorFor(x=>x.DoctorId);
  [Fact] public void CreateValidate_InvalidOverallRating_Fails()=>_c.TestValidate(new CreatePatientReviewDTO{DoctorId="d1",OverallRating=6}).ShouldHaveValidationErrorFor(x=>x.OverallRating);
  [Fact] public void CreateValidate_ReviewTooLong_Fails()=>_c.TestValidate(new CreatePatientReviewDTO{DoctorId="d1",Review=new string('x',2001)}).ShouldHaveValidationErrorFor(x=>x.Review);
  [Fact] public void UpdateValidate_ValidDto_Passes()=>_u.TestValidate(new UpdatePatientReviewDTO{DoctorId="d1",OverallRating=3}).ShouldNotHaveAnyValidationErrors();
  [Fact] public void UpdateValidate_InvalidWaitTimeRating_Fails()=>_u.TestValidate(new UpdatePatientReviewDTO{DoctorId="d1",WaitTimeRating=0}).ShouldHaveValidationErrorFor(x=>x.WaitTimeRating);
  [Fact] public void UpdateValidate_MissingDoctorId_Fails()=>_u.TestValidate(new UpdatePatientReviewDTO{OverallRating=4}).ShouldHaveValidationErrorFor(x=>x.DoctorId);
}
