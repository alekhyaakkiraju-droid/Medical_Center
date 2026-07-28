using AngularApi.Contracts.Enums;
using AngularApi.Models;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AngularApi.Tests.Services;

public class AppointmentServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        _service = new AppointmentService(
            _context,
            Microsoft.Extensions.Options.Options.Create(new AppointmentSettings { DefaultFee = 55, DefaultCenterId = 9 }));
    }

    [Fact]
    public async Task CreateAppointmentAsync_ValidDoctor_AppliesDefaultsAndPersists()
    {
        _context.Doctors.Add(new Doctor { Id = "doctor-id", Name = "Dr. Smith" });
        await _context.SaveChangesAsync();

        var appointment = new Appointment { DoctorId = "doctor-id", AppointmentTakenDate = DateTime.UtcNow };

        var (created, error) = await _service.CreateAppointmentAsync(appointment, "patient1");

        error.Should().BeNull();
        created.Should().NotBeNull();
        created!.DoctorName.Should().Be("Dr. Smith");
        created.PatientId.Should().Be("patient1");
        created.MedicalCenterId.Should().Be(9);
        created.Amount.Should().Be(55);
        created.AppointmentStatusId.Should().Be((int)AppointmentStatusEnum.Active);
        created.PaymentStatus.Should().Be("Pending");
        (await _context.Appointments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateAppointmentAsync_InvalidDoctorId_ReturnsError()
    {
        var appointment = new Appointment { DoctorId = "missing-doctor" };

        var (created, error) = await _service.CreateAppointmentAsync(appointment, "patient1");

        created.Should().BeNull();
        error.Should().Be("Invalid DoctorId");
    }

    [Fact]
    public async Task CreateAppointmentAsync_MissingDoctorId_ReturnsError()
    {
        var appointment = new Appointment();

        var (created, error) = await _service.CreateAppointmentAsync(appointment, "patient1");

        created.Should().BeNull();
        error.Should().Be("DoctorId is required");
    }

    [Fact]
    public async Task UpdateAppointmentAsync_ValidInput_UpdatesAppointment()
    {
        var appointment = new Appointment { Id = 1, AppointmentTakenDate = DateTime.Now };
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var dto = new UpdateAppointmentDTO { Id = 1, AppointmentTakenDate = DateTime.Now.AddDays(1) };

        var updated = await _service.UpdateAppointmentAsync(1, dto);

        updated.Should().BeTrue();
        var dbAppointment = await _context.Appointments.FindAsync(1);
        dbAppointment!.AppointmentTakenDate.Should().Be(dto.AppointmentTakenDate);
    }

    [Fact]
    public async Task GetTodaysAppointmentsAsync_ReturnsOnlyTodaysAppointments()
    {
        var today = DateTime.Today;
        _context.Appointments.AddRange(
            new Appointment { Id = 1, ProbableStartTime = today },
            new Appointment { Id = 2, ProbableStartTime = today.AddDays(1) });
        await _context.SaveChangesAsync();

        var result = await _service.GetTodaysAppointmentsAsync(new PaginationParameters());

        result.Items.Should().HaveCount(1);
        result.Items[0].AppointmentId.Should().Be(1);
    }

    [Fact]
    public async Task GetTotalEarningsAsync_ReturnsSumOfAmounts()
    {
        _context.Appointments.AddRange(
            new Appointment { Id = 1, Amount = 30 },
            new Appointment { Id = 2, Amount = 50 });
        await _context.SaveChangesAsync();

        var total = await _service.GetTotalEarningsAsync();

        total.Should().Be(80);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
