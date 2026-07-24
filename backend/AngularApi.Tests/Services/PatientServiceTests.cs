using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.Services;

public class PatientServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        _service = new PatientService(_context);
    }

    [Fact]
    public async Task UpdatePatientAsync_ValidInput_UpdatesPatient()
    {
        var patient = new Patient { Id = "patient1", Name = "John Doe", Email = "john@example.com", Image = "old.jpg" };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var dto = new PatientDTO { PatientId = "patient1", Name = "John Updated", Email = "john.updated@example.com", Image = "new.jpg" };

        var updated = await _service.UpdatePatientAsync("patient1", dto);

        updated.Should().BeTrue();
        var dbPatient = await _context.Patients.FindAsync("patient1");
        dbPatient!.Name.Should().Be("John Updated");
        dbPatient.Email.Should().Be("john.updated@example.com");
        dbPatient.Image.Should().Be("new.jpg");
    }

    [Fact]
    public async Task DeletePatientAppointmentAsync_ValidIds_RemovesAppointment()
    {
        var appointment = new Appointment { Id = 1, PatientId = "patient1", AppointmentTakenDate = DateTime.Now };
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var deleted = await _service.DeletePatientAppointmentAsync("patient1", 1);

        deleted.Should().BeTrue();
        (await _context.Appointments.FindAsync(1)).Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
