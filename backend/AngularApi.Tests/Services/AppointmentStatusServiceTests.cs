using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.Services;

public class AppointmentStatusServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly AppointmentStatusService _service;

    public AppointmentStatusServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        _service = new AppointmentStatusService(_context);

        _context.AppointmentStatus.AddRange(
            new AppointmentStatus { Id = 1, Status = AppointmentStatusEnum.Active },
            new AppointmentStatus { Id = 2, Status = AppointmentStatusEnum.Complete },
            new AppointmentStatus { Id = 3, Status = AppointmentStatusEnum.Canceled });
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedAppointmentStatuses()
    {
        var result = await _service.GetAllAsync(new PaginationParameters());

        result.Items.Should().HaveCount(3);
        result.Items.Should().Contain(s => s.Id == 1 && s.Status == AppointmentStatusEnum.Active);
        result.Items.Should().Contain(s => s.Id == 2 && s.Status == AppointmentStatusEnum.Complete);
        result.Items.Should().Contain(s => s.Id == 3 && s.Status == AppointmentStatusEnum.Canceled);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAppointmentStatus()
    {
        var result = await _service.GetByIdAsync(2);

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Status.Should().Be(AppointmentStatusEnum.Complete);
    }

    [Fact]
    public async Task GetByIdAsync_MissingId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidInput_PersistsAppointmentStatus()
    {
        var appointmentStatus = new AppointmentStatus { Status = AppointmentStatusEnum.Active };

        var created = await _service.CreateAsync(appointmentStatus);

        created.Id.Should().BeGreaterThan(0);
        created.Status.Should().Be(AppointmentStatusEnum.Active);
        (await _context.AppointmentStatus.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesAppointmentStatus()
    {
        var appointmentStatus = new AppointmentStatus { Id = 1, Status = AppointmentStatusEnum.Canceled };

        var updated = await _service.UpdateAsync(1, appointmentStatus);

        updated.Should().BeTrue();
        var dbStatus = await _context.AppointmentStatus.FindAsync(1);
        dbStatus!.Status.Should().Be(AppointmentStatusEnum.Canceled);
    }

    [Fact]
    public async Task UpdateAsync_IdMismatch_ReturnsFalse()
    {
        var appointmentStatus = new AppointmentStatus { Id = 2, Status = AppointmentStatusEnum.Active };

        var updated = await _service.UpdateAsync(1, appointmentStatus);

        updated.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesAppointmentStatus()
    {
        var deleted = await _service.DeleteAsync(3);

        deleted.Should().BeTrue();
        (await _context.AppointmentStatus.FindAsync(3)).Should().BeNull();
        (await _context.AppointmentStatus.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_MissingId_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(999);

        deleted.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
