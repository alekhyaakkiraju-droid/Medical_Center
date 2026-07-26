using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.Services;

public class MedicalCenterServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly MedicalCenterService _service;

    public MedicalCenterServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        _service = new MedicalCenterService(_context);
    }

    [Fact]
    public async Task GetMedicalCentersAsync_ReturnsPagedMedicalCenterListItems()
    {
        _context.MedicalCenter.AddRange(
            new MedicalCenter { Id = 1, City = "Boston", State = "MA", StreetAddress = "1 Main St", Zip = "02101" },
            new MedicalCenter { Id = 2, City = "Cambridge", State = "MA", StreetAddress = "2 Oak Ave", Zip = "02139" });
        await _context.SaveChangesAsync();
        var result = await _service.GetMedicalCentersAsync(new PaginationParameters());
        result.Items.Should().HaveCount(2);
        result.Items[0].City.Should().Be("Boston");
        result.Items[1].City.Should().Be("Cambridge");
    }

    [Fact]
    public async Task GetMedicalCenterByIdAsync_ExistingId_ReturnsMedicalCenter()
    {
        _context.MedicalCenter.Add(new MedicalCenter { Id = 1, City = "Boston", State = "MA" });
        await _context.SaveChangesAsync();
        var result = await _service.GetMedicalCenterByIdAsync(1);
        result.Should().NotBeNull();
        result!.City.Should().Be("Boston");
    }

    [Fact]
    public async Task GetMedicalCenterByIdAsync_NonExistingId_ReturnsNull()
    {
        (await _service.GetMedicalCenterByIdAsync(999)).Should().BeNull();
    }

    [Fact]
    public async Task CreateMedicalCenterAsync_ValidInput_PersistsMedicalCenter(){var dto=new CreateMedicalCenterDTO{StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101",FirstConsultationFee=100m,FollowupConsultationFee=75m};var created=await _service.CreateMedicalCenterAsync(dto);created.Id.Should().BeGreaterThan(0);created.City.Should().Be("Boston");(await _context.MedicalCenter.FindAsync(created.Id)).Should().NotBeNull();}

    [Fact]
    public async Task UpdateMedicalCenterAsync_ValidInput_UpdatesMedicalCenter(){var center=new MedicalCenter{Id=1,StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101"};_context.MedicalCenter.Add(center);await _context.SaveChangesAsync();var dto=new UpdateMedicalCenterDTO{StreetAddress="2 Oak Ave",City="Cambridge",State="MA",Zip="02139"};(await _service.UpdateMedicalCenterAsync(1,dto)).Should().BeTrue();(await _context.MedicalCenter.FindAsync(1))!.City.Should().Be("Cambridge");}

    [Fact]
    public async Task UpdateMedicalCenterAsync_ExistingIdWithOptionalFields_Updates(){var center=new MedicalCenter{Id=1,StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101"};_context.MedicalCenter.Add(center);await _context.SaveChangesAsync();(await _service.UpdateMedicalCenterAsync(1,new UpdateMedicalCenterDTO{StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101",HospitalAffiliationId=5})).Should().BeTrue();}

    [Fact]
    public async Task DeleteMedicalCenterAsync_ExistingId_RemovesMedicalCenter()
    {
        _context.MedicalCenter.Add(new MedicalCenter { Id = 1, City = "Boston" });
        await _context.SaveChangesAsync();
        (await _service.DeleteMedicalCenterAsync(1)).Should().BeTrue();
        (await _context.MedicalCenter.FindAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteMedicalCenterAsync_NonExistingId_ReturnsFalse()
    {
        (await _service.DeleteMedicalCenterAsync(1)).Should().BeFalse();
    }

    public void Dispose() { _context.Database.EnsureDeleted(); _context.Dispose(); }
}
