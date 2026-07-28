using AngularApi.Models;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.Services;

public class SpecializationServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly SpecializationService _service;
    public SpecializationServiceTests()
    {
        _context = new MedicalCenterDbContext(new DbContextOptionsBuilder<MedicalCenterDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _service = new SpecializationService(_context);
    }

    [Fact]
    public async Task GetSpecializationsAsync_ReturnsPagedSpecializationListItems()
    {
        _context.Specializations.AddRange(
            new Specialization { Id = 1, SpecializationName = "Cardiology", Services = new List<Service> { new() { Id = 1, Name = "Heart Checkup" } } },
            new Specialization { Id = 2, SpecializationName = "Neurology" });
        await _context.SaveChangesAsync();
        var result = await _service.GetSpecializationsAsync(new PaginationParameters());
        result.Items.Should().HaveCount(2);
        result.Items[0].SpecializationName.Should().Be("Cardiology");
    }

    [Fact]
    public async Task GetSpecializationByIdAsync_ExistingId_ReturnsDetailDtoWithServices()
    {
        _context.Specializations.Add(new Specialization { Id = 1, SpecializationName = "Cardiology", Services = new List<Service> { new() { Id = 1, Name = "Heart Checkup" } } });
        await _context.SaveChangesAsync();
        var result = await _service.GetSpecializationByIdAsync(1);
        result!.Services.Should().ContainSingle(s => s.Name == "Heart Checkup");
        result.SpecializationName.Should().Be("Cardiology");
    }

    [Fact]
    public async Task GetSpecializationByIdAsync_NonExistingId_ReturnsNull() =>
        (await _service.GetSpecializationByIdAsync(99)).Should().BeNull();

    [Fact]
    public async Task CreateSpecializationAsync_ValidInput_PersistsSpecialization()
    {
        var created = await _service.CreateSpecializationAsync(new CreateSpecializationDTO { SpecializationName = "Cardiology" });
        created.SpecializationName.Should().Be("Cardiology");
        created.Id.Should().BeGreaterThan(0);
        (await _context.Specializations.FindAsync(created.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateSpecializationAsync_ValidInput_UpdatesSpecialization()
    {
        _context.Specializations.Add(new Specialization { Id = 1, SpecializationName = "Cardiology" });
        await _context.SaveChangesAsync();
        (await _service.UpdateSpecializationAsync(1, new UpdateSpecializationDTO { SpecializationName = "Pediatric Cardiology" })).Should().BeTrue();
        (await _context.Specializations.FindAsync(1))!.SpecializationName.Should().Be("Pediatric Cardiology");
    }

    [Fact]
    public async Task UpdateSpecializationAsync_NonExistingId_ReturnsFalse() =>
        (await _service.UpdateSpecializationAsync(999, new UpdateSpecializationDTO { SpecializationName = "Neurology" })).Should().BeFalse();

    [Fact]
    public async Task UpdateSpecializationAsync_MissingRecord_ReturnsFalse() =>
        (await _service.UpdateSpecializationAsync(1, new UpdateSpecializationDTO { SpecializationName = "Cardiology" })).Should().BeFalse();

    [Fact]
    public async Task DeleteSpecializationAsync_ExistingId_RemovesSpecialization()
    {
        _context.Specializations.Add(new Specialization { Id = 1, SpecializationName = "Cardiology" });
        await _context.SaveChangesAsync();
        (await _service.DeleteSpecializationAsync(1)).Should().BeTrue();
        (await _context.Specializations.FindAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteSpecializationAsync_NonExistingId_ReturnsFalse() =>
        (await _service.DeleteSpecializationAsync(1)).Should().BeFalse();

    public void Dispose() { _context.Database.EnsureDeleted(); _context.Dispose(); }
}
