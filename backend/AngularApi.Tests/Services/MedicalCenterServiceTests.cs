using AngularApi.DTO; using AngularApi.Models; using AngularApi.Services.impelementation; using AngularApi.Services.Interfaces; using FluentAssertions; using Microsoft.EntityFrameworkCore; using Microsoft.Extensions.Logging.Abstractions; using System.Security.Claims;
namespace AngularApi.Tests.Services;
public class MedicalCenterServiceTests : IDisposable {
  private readonly MedicalCenterDbContext _context; private readonly MedicalCenterService _service;
  public MedicalCenterServiceTests(){_context=new MedicalCenterDbContext(new DbContextOptionsBuilder<MedicalCenterDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options); _service=new MedicalCenterService(_context,new OwnershipValidator(),NullLogger<MedicalCenterService>.Instance);}
  [Fact] public async Task CreateMedicalCenterAsync_NonAdminUser_ReturnsForbidden(){var (created,result)=await _service.CreateMedicalCenterAsync(new CreateMedicalCenterDTO{StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101"},User("doctor-1","doctor")); result.Should().Be(ResourceMutationResult.Forbidden); created.Should().BeNull();}
  [Fact] public async Task UpdateMedicalCenterAsync_NonAdminUser_ReturnsForbidden(){_context.MedicalCenter.Add(new MedicalCenter{Id=1,StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101"}); await _context.SaveChangesAsync(); (await _service.UpdateMedicalCenterAsync(1,new UpdateMedicalCenterDTO{StreetAddress="2 Oak Ave",City="Cambridge",State="MA",Zip="02139"},User("doctor-1","doctor"))).Should().Be(ResourceMutationResult.Forbidden);}
  static ClaimsPrincipal User(string id,string role)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,id),new Claim(ClaimTypes.Role,role)},"TestAuth"));
  public void Dispose(){_context.Database.EnsureDeleted(); _context.Dispose();}
}
