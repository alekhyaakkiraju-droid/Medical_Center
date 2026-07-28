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
    public async Task SelectDoctorDetailDto_ProjectsPublicFieldsWithoutIdentityOrAuditData()
    {
        var practicingFrom = new DateTime(2012, 3, 15);
        _context.Doctors.Add(new Doctor
        {
            Id = "doctor1",
            Name = "Dr Smith",
            Image = "avatar.png",
            ProfessionalStatement = "Board-certified surgeon",
            PracticingFrom = practicingFrom,
            PasswordHash = "must-not-leak",
            SecurityStamp = "must-not-leak",
            NormalizedEmail = "DR@EXAMPLE.COM",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin",
            DoctorSpecializations = new List<DoctorSpecialization>
            {
                new() { Specialization = new Specialization { SpecializationName = "Cardiology" } },
                new() { Specialization = new Specialization { SpecializationName = "Surgery" } }
            },
            Qualifications = new List<DoctorQualification>
            {
                new() { QualificationName = "MD", InstituteName = "Stanford", ProcurementYear = new DateTime(2008, 5, 1) },
                new() { QualificationName = "FACS", InstituteName = "ACS", ProcurementYear = new DateTime(2012, 5, 1) }
            },
            HospitalAffiliations = new List<HospitalAffiliation>
            {
                new()
                {
                    HospitalName = "City Hospital",
                    City = "Chicago",
                    Country = "USA",
                    StartDate = new DateTime(2015, 1, 1),
                    EndDate = null
                }
            },
            PatientReviews = new List<PatientReview>
            {
                new() { OverallRating = 4 },
                new() { OverallRating = 2 }
            }
        });
        await _context.SaveChangesAsync();

        var detail = await _context.Doctors.SelectDoctorDetailDto().SingleAsync();

        detail.Id.Should().Be("doctor1");
        detail.Name.Should().Be("Dr Smith");
        detail.Image.Should().Be("avatar.png");
        detail.ProfessionalStatement.Should().Be("Board-certified surgeon");
        detail.PracticingFrom.Should().Be(practicingFrom);
        detail.Specializations.Should().BeEquivalentTo("Cardiology", "Surgery");
        detail.Qualifications.Should().HaveCount(2);
        detail.Qualifications![0].QualificationName.Should().Be("MD");
        detail.Qualifications[0].InstituteName.Should().Be("Stanford");
        detail.HospitalAffiliations.Should().ContainSingle(h =>
            h.HospitalName == "City Hospital" && h.City == "Chicago" && h.Country == "USA");
        detail.AverageRating.Should().Be(3.0);
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

    [Fact]
    public async Task SelectSpecializationDetailDto_ProjectsServicesWithoutNavigationGraph()
    {
        _context.Specializations.Add(new Specialization
        {
            Id = 1,
            SpecializationName = "Cardiology",
            Services = new List<Service> { new() { Id = 10, Name = "Heart Checkup", Description = "Full screening" } }
        });
        await _context.SaveChangesAsync();

        var result = await _context.Specializations.SelectSpecializationDetailDto().SingleAsync();

        result.SpecializationName.Should().Be("Cardiology");
        result.Services.Should().ContainSingle(s => s.Name == "Heart Checkup" && s.Description == "Full screening");
    }

    [Fact]
    public async Task SelectAppointmentStatusDetailDto_ProjectsStatusFieldsOnly()
    {
        _context.AppointmentStatus.Add(new AppointmentStatus { Id = 1, Status = AppointmentStatusEnum.Complete });
        await _context.SaveChangesAsync();

        var result = await _context.AppointmentStatus.SelectAppointmentStatusDetailDto().SingleAsync();

        result.Id.Should().Be(1);
        result.Status.Should().Be(AppointmentStatusEnum.Complete);
    }

    [Fact]
    public async Task SelectMedicalCenterDetailDto_ProjectsScalarFieldsWithoutNavigationGraph()
    {
        _context.MedicalCenter.Add(new MedicalCenter
        {
            Id = 1,
            City = "Boston",
            State = "MA",
            HospitalAffiliation = new HospitalAffiliation { HospitalName = "General Hospital" }
        });
        await _context.SaveChangesAsync();

        var result = await _context.MedicalCenter.SelectMedicalCenterDetailDto().SingleAsync();

        result.City.Should().Be("Boston");
        result.State.Should().Be("MA");
    }

    [Fact]
    public async Task SelectMedicalCenterDoctorAvailabilityDetailDto_ProjectsAvailabilityWithoutMedicalCenter()
    {
        var center = new MedicalCenter { Id = 1, City = "Boston" };
        _context.MedicalCenterDoctorAvailability.Add(new MedicalCenterDoctorAvailability
        {
            Id = 1,
            MedicalCenterId = center.Id,
            MedicalCenter = center,
            DayOfWeek = "Monday",
            IsAvailable = true
        });
        await _context.SaveChangesAsync();

        var result = await _context.MedicalCenterDoctorAvailability
            .SelectMedicalCenterDoctorAvailabilityDetailDto()
            .SingleAsync();

        result.DayOfWeek.Should().Be("Monday");
        result.MedicalCenterId.Should().Be(1);
    }

    [Fact]
    public async Task SelectPatientReviewDetailDto_HidesPatientIdWhenAnonymous()
    {
        _context.PatientReviews.AddRange(
            new PatientReview { Id = 1, PatientId = "p1", DoctorId = "d1", IsReviewAnonymous = true, OverallRating = 5, CreatedBy = "p1" },
            new PatientReview { Id = 2, PatientId = "p2", DoctorId = "d1", IsReviewAnonymous = false, OverallRating = 4, CreatedBy = "p2" });
        await _context.SaveChangesAsync();

        var reviews = await _context.PatientReviews.SelectPatientReviewDetailDto().OrderBy(r => r.Id).ToListAsync();

        reviews[0].PatientId.Should().BeNull();
        reviews[1].PatientId.Should().Be("p2");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
