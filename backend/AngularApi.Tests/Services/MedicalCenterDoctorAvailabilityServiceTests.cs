using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.impelementation;
using AngularApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AngularApi.Tests.Services;

public class MedicalCenterDoctorAvailabilityServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly MedicalCenterDoctorAvailabilityService _service;

    public MedicalCenterDoctorAvailabilityServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        _service = new MedicalCenterDoctorAvailabilityService(_context, new Mock<IOwnershipValidator>().Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedAvailabilityRecords()
    {
        var medicalCenter = SeedMedicalCenter();
        _context.MedicalCenterDoctorAvailability.AddRange(
            CreateAvailability(medicalCenter.Id, "Monday"),
            CreateAvailability(medicalCenter.Id, "Tuesday"));
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(new PaginationParameters { Page = 1, PageSize = 10 });

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(a => a.MedicalCenterId == medicalCenter.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAvailability()
    {
        var medicalCenter = SeedMedicalCenter();
        var availability = CreateAvailability(medicalCenter.Id, "Wednesday");
        _context.MedicalCenterDoctorAvailability.Add(availability);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(availability.Id);

        result.Should().NotBeNull();
        result!.DayOfWeek.Should().Be("Wednesday");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidMedicalCenter_PersistsAvailability()
    {
        var medicalCenter = SeedMedicalCenter();
        var created = await _service.CreateAsync(CreateDto(medicalCenter.Id, "Thursday"));

        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        (await _context.MedicalCenterDoctorAvailability.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_InvalidMedicalCenterId_ReturnsNull()
    {
        var created = await _service.CreateAsync(CreateDto(999, "Friday"));

        created.Should().BeNull();
        (await _context.MedicalCenterDoctorAvailability.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAvailability_UpdatesRecord()
    {
        var medicalCenter = SeedMedicalCenter();
        var availability = CreateAvailability(medicalCenter.Id, "Saturday");
        _context.MedicalCenterDoctorAvailability.Add(availability);
        await _context.SaveChangesAsync();

        var dto = new UpdateMedicalCenterDoctorAvailabilityDTO { MedicalCenterId = medicalCenter.Id, DayOfWeek = availability.DayOfWeek!, StartTime = availability.StartTime!.Value, EndTime = availability.EndTime!.Value, IsAvailable = false, ReasonOfUnavailability = "Holiday" };
        var updated = await _service.UpdateAsync(availability.Id, dto);

        updated.Should().BeTrue();
        var dbAvailability = await _context.MedicalCenterDoctorAvailability.FindAsync(availability.Id);
        dbAvailability!.IsAvailable.Should().BeFalse();
        dbAvailability.ReasonOfUnavailability.Should().Be("Holiday");
    }

    [Fact]
    public async Task UpdateAsync_IdMismatch_ReturnsFalse()
    {
        var medicalCenter = SeedMedicalCenter();
        var availability = CreateAvailability(medicalCenter.Id, "Sunday");
        _context.MedicalCenterDoctorAvailability.Add(availability);
        await _context.SaveChangesAsync();

        var updated = await _service.UpdateAsync(availability.Id + 1, CreateUpdateDto(medicalCenter.Id, "Sunday"));

        updated.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingAvailability_RemovesRecord()
    {
        var medicalCenter = SeedMedicalCenter();
        var availability = CreateAvailability(medicalCenter.Id, "Monday");
        _context.MedicalCenterDoctorAvailability.Add(availability);
        await _context.SaveChangesAsync();

        var deleted = await _service.DeleteAsync(availability.Id);

        deleted.Should().BeTrue();
        (await _context.MedicalCenterDoctorAvailability.FindAsync(availability.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(404);

        deleted.Should().BeFalse();
    }

    private MedicalCenter SeedMedicalCenter()
    {
        var medicalCenter = new MedicalCenter
        {
            StreetAddress = "123 Main St",
            City = "Test City",
            State = "TS",
            Zip = "12345"
        };
        _context.MedicalCenter.Add(medicalCenter);
        _context.SaveChanges();
        return medicalCenter;
    }

    private static MedicalCenterDoctorAvailability CreateAvailability(int medicalCenterId, string dayOfWeek) =>
        new()
        {
            MedicalCenterId = medicalCenterId,
            DayOfWeek = dayOfWeek,
            StartTime = DateTime.Today.AddHours(9),
            EndTime = DateTime.Today.AddHours(17),
            IsAvailable = true
        };

    private static CreateMedicalCenterDoctorAvailabilityDTO CreateDto(int medicalCenterId, string dayOfWeek) => new()
    {
        MedicalCenterId = medicalCenterId,
        DayOfWeek = dayOfWeek,
        StartTime = DateTime.Today.AddHours(9),
        EndTime = DateTime.Today.AddHours(17),
        IsAvailable = true
    };

    private static UpdateMedicalCenterDoctorAvailabilityDTO CreateUpdateDto(int medicalCenterId, string dayOfWeek) => new()
    {
        MedicalCenterId = medicalCenterId,
        DayOfWeek = dayOfWeek,
        StartTime = DateTime.Today.AddHours(9),
        EndTime = DateTime.Today.AddHours(17),
        IsAvailable = true
    };

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
