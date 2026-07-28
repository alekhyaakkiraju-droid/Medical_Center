using AngularApi.Contracts.Enums;
using AngularApi.Models;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.Services;

public class DoctorServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        _service = new DoctorService(_context);
    }

    [Fact]
    public async Task GetBookingsAsync_ReturnsOnlyActiveBookingsForDoctor()
    {
        _context.Appointments.AddRange(
            new Appointment
            {
                Id = 1,
                DoctorId = "doctor1",
                AppointmentStatus = new AppointmentStatus { Status = AppointmentStatusEnum.Active },
                Patient = new Patient { UserName = "Patient1" }
            },
            new Appointment
            {
                Id = 2,
                DoctorId = "doctor1",
                AppointmentStatus = new AppointmentStatus { Status = AppointmentStatusEnum.Canceled }
            });
        await _context.SaveChangesAsync();

        var result = await _service.GetBookingsAsync("doctor1", new PaginationParameters());

        result.Items.Should().HaveCount(1);
        result.Items[0].AppointmentId.Should().Be(1);
    }

    [Fact]
    public async Task CancelDoctorAppointmentAsync_ExistingAppointment_SetsStatusToCanceled()
    {
        var appointment = new Appointment
        {
            Id = 1,
            DoctorId = "doctor1",
            AppointmentStatus = new AppointmentStatus { Status = AppointmentStatusEnum.Active },
            AppointmentStatusId = 1
        };
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var canceled = await _service.CancelDoctorAppointmentAsync("doctor1", 1);

        canceled.Should().BeTrue();
        var dbAppointment = await _context.Appointments.FindAsync(1);
        dbAppointment!.AppointmentStatusId.Should().Be((int)AppointmentStatusEnum.Canceled);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
