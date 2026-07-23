using AngularApi.DTO;
using AngularApi.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.DTO;

public class QueryProjectionsTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;

    public QueryProjectionsTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
    }

    [Fact]
    public async Task SelectDoctorDto_ProjectsOnlyDoctorFields()
    {
        _context.Doctors.Add(new Doctor
        {
            Id = "doctor1",
            Name = "Dr Smith",
            PasswordHash = "secret",
            DoctorSpecializations = new List<DoctorSpecialization>
            {
                new() { Specialization = new Specialization { SpecializationName = "Cardiology" } }
            }
        });
        await _context.SaveChangesAsync();

        var doctors = await _context.Doctors.SelectDoctorDto().ToListAsync();

        doctors.Should().ContainSingle();
        doctors[0].Name.Should().Be("Dr Smith");
        doctors[0].Specializations.Should().Contain("Cardiology");
    }

    [Fact]
    public async Task SelectBookingDto_ProjectsBookingFieldsWithoutNavigationGraph()
    {
        _context.Appointments.Add(new Appointment
        {
            Id = 1,
            DoctorId = "doctor1",
            Patient = new Patient { UserName = "Jane Doe" },
            AppointmentStatus = new AppointmentStatus { Status = AppointmentStatusEnum.Active }
        });
        await _context.SaveChangesAsync();

        var bookings = await _context.Appointments.SelectBookingDto().ToListAsync();

        bookings.Should().ContainSingle();
        bookings[0].AppointmentId.Should().Be(1);
        bookings[0].PatientName.Should().Be("Jane Doe");
        bookings[0].AppointmentStatus.Should().Be("Active");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
