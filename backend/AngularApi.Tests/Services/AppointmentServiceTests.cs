using AngularApi.Contracts.Enums;
using AngularApi.Models;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Services.impelementation;
using AngularApi.Contracts.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace AngularApi.Tests.Services;

public class AppointmentServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly EmailTemplateService _emailTemplateService;

    public AppointmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        _emailServiceMock = new Mock<IEmailService>();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var templateDir = Path.Combine(tempDir, "EmailTemplates");
        Directory.CreateDirectory(templateDir);
        File.WriteAllText(
            Path.Combine(templateDir, "ConfirmAppointment.html"),
            "Hello {{patientName}} with {{DoctorName}} on {{date}}");

        var webHostEnvironmentMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(tempDir);
        _emailTemplateService = new EmailTemplateService(webHostEnvironmentMock.Object);
    }

    private AppointmentService CreateService() =>
        new(
            _context,
            Microsoft.Extensions.Options.Options.Create(new AppointmentSettings { DefaultFee = 55, DefaultCenterId = 9 }),
            _emailServiceMock.Object,
            _emailTemplateService,
            NullLogger<AppointmentService>.Instance);

    [Fact]
    public async Task CreateAppointmentAsync_ValidDoctor_AppliesDefaultsAndPersists()
    {
        _context.Doctors.Add(new Doctor { Id = "doctor-id", Name = "Dr. Smith" });
        await _context.SaveChangesAsync();

        var appointment = new Appointment
        {
            DoctorId = "doctor-id",
            AppointmentTakenDate = DateTime.UtcNow,
            Email = "patient@example.com",
            Name = "Jane Patient",
        };

        var (created, error) = await CreateService().CreateAppointmentAsync(appointment, "patient1");

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
    public async Task CreateAppointmentAsync_SendsConfirmationEmailWithExpectedContent()
    {
        _context.Doctors.Add(new Doctor { Id = "doctor-id", Name = "Dr. Smith" });
        await _context.SaveChangesAsync();
        Message? captured = null;
        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<Message>()))
            .Callback<Message>(message => captured = message)
            .Returns(Task.CompletedTask);

        var appointmentDate = new DateTime(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc);
        var appointment = new Appointment
        {
            DoctorId = "doctor-id",
            AppointmentTakenDate = appointmentDate,
            Email = "patient@example.com",
            Name = "Jane Patient",
        };

        await CreateService().CreateAppointmentAsync(appointment, "patient1");

        captured.Should().NotBeNull();
        captured!.To.Should().Contain("patient@example.com");
        captured.Subject.Should().Be("Appointment Confirmation - CareShift");
        captured.Body.Should().Contain("Jane Patient");
        captured.Body.Should().Contain("Dr. Smith");
        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<Message>()), Times.Once);
    }

    [Fact]
    public async Task CreateAppointmentAsync_EmailFailureStillCreatesAppointment()
    {
        _context.Doctors.Add(new Doctor { Id = "doctor-id", Name = "Dr. Smith" });
        await _context.SaveChangesAsync();
        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<Message>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var appointment = new Appointment
        {
            DoctorId = "doctor-id",
            AppointmentTakenDate = DateTime.UtcNow,
            Email = "patient@example.com",
            Name = "Jane Patient",
        };

        var (created, error) = await CreateService().CreateAppointmentAsync(appointment, "patient1");

        error.Should().BeNull();
        created.Should().NotBeNull();
        (await _context.Appointments.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateAppointmentAsync_InvalidDoctorId_ReturnsError()
    {
        var appointment = new Appointment { DoctorId = "missing-doctor" };

        var (created, error) = await CreateService().CreateAppointmentAsync(appointment, "patient1");

        created.Should().BeNull();
        error.Should().Be("Invalid DoctorId");
    }

    [Fact]
    public async Task CreateAppointmentAsync_MissingDoctorId_ReturnsError()
    {
        var appointment = new Appointment();

        var (created, error) = await CreateService().CreateAppointmentAsync(appointment, "patient1");

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

        var updated = await CreateService().UpdateAppointmentAsync(1, dto);

        updated.Should().BeTrue();
        (await _context.Appointments.FindAsync(1))!.AppointmentTakenDate.Should().Be(dto.AppointmentTakenDate);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
