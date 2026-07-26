using AngularApi.DTO; using AngularApi.Models; using AngularApi.Services.impelementation; using AngularApi.Services.Interfaces; using FluentAssertions; using Microsoft.EntityFrameworkCore; using Microsoft.Extensions.Logging.Abstractions; using System.Security.Claims;
namespace AngularApi.Tests.Services;
public class PatientReviewServiceTests : IDisposable {
  private readonly MedicalCenterDbContext _context; private readonly PatientReviewService _service;
  public PatientReviewServiceTests(){_context=new MedicalCenterDbContext(new DbContextOptionsBuilder<MedicalCenterDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options); _service=new PatientReviewService(_context,new OwnershipValidator(),NullLogger<PatientReviewService>.Instance);}
  [Fact] public async Task UpdateAsync_OtherPatientReview_Denied(){Seed(1,"p2",3); (await _service.UpdateAsync(1,new UpdatePatientReviewDTO{OverallRating=5},User("p1","user"))).Should().Be(ResourceMutationResult.Forbidden);}
  [Fact] public async Task DeleteAsync_OtherPatientReview_Denied(){Seed(1,"p2",5); (await _service.DeleteAsync(1,User("p1","user"))).Should().Be(ResourceMutationResult.Forbidden);}
  void Seed(int id,string pid,int rating){_context.PatientReviews.Add(new PatientReview{Id=id,PatientId=pid,DoctorId="d1",OverallRating=rating}); _context.SaveChanges();}
  static ClaimsPrincipal User(string id,string role)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,id),new Claim(ClaimTypes.Role,role)},"TestAuth"));
  public void Dispose(){_context.Database.EnsureDeleted(); _context.Dispose();}
}
