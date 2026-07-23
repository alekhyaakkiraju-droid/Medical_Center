using System.Text.Json;
using AngularApi.DTO;
using AngularApi.Models;
using FluentAssertions;

namespace AngularApi.Tests.DTO;

public class ListEndpointPayloadTests
{
    [Fact]
    public void ProjectedDoctorDto_IsAtLeast40PercentSmallerThanDoctorEntityGraph()
    {
        var doctor = CreateSampleDoctorEntity();
        var entityPayload = JsonSerializer.Serialize(new[] { doctor });
        var projectedPayload = JsonSerializer.Serialize(new[]
        {
            new DoctorDTO
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Image = doctor.Image,
                ProfessionalStatement = doctor.ProfessionalStatement,
                PracticingFrom = doctor.PracticingFrom,
                Specializations = new List<string> { "Cardiology" }
            }
        });

        projectedPayload.Length.Should().BeLessThan((int)(entityPayload.Length * 0.6),
            "projected list payloads should be at least 40% smaller than full entity graphs");
    }

    [Fact]
    public void ProjectedBookingDto_IsAtLeast40PercentSmallerThanAppointmentEntityGraph()
    {
        var appointment = CreateSampleAppointmentEntity();
        var entityPayload = JsonSerializer.Serialize(new[] { appointment });
        var projectedPayload = JsonSerializer.Serialize(new[]
        {
            new BookingDTO
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient?.UserName,
                DoctorName = appointment.DoctorName,
                AppointmentTakenDate = appointment.AppointmentTakenDate,
                AppointmentStatus = appointment.AppointmentStatus?.Status?.ToString()
            }
        });

        projectedPayload.Length.Should().BeLessThan((int)(entityPayload.Length * 0.6),
            "projected list payloads should be at least 40% smaller than full entity graphs");
    }

    [Fact]
    public void ProjectedListPayloads_DoNotContainIdentityNavigationFields()
    {
        var doctorJson = JsonSerializer.Serialize(new DoctorDTO { Id = "d1", Name = "Dr Smith" });
        var bookingJson = JsonSerializer.Serialize(new BookingDTO { AppointmentId = 1, PatientName = "Jane" });

        doctorJson.Should().NotContain("passwordHash");
        doctorJson.Should().NotContain("normalizedEmail");
        bookingJson.Should().NotContain("patient.");
        bookingJson.Should().NotContain("doctorSpecializations");
    }

    private static Doctor CreateSampleDoctorEntity() =>
        new()
        {
            Id = "doctor1",
            UserName = "doctor.user",
            NormalizedUserName = "DOCTOR.USER",
            Email = "doctor@example.com",
            NormalizedEmail = "DOCTOR@EXAMPLE.COM",
            PasswordHash = "hashed-password-value",
            SecurityStamp = "security-stamp",
            ConcurrencyStamp = "concurrency-stamp",
            Name = "Dr Smith",
            Image = "image.png",
            ProfessionalStatement = "Experienced cardiologist",
            PracticingFrom = DateTime.UtcNow.AddYears(-10),
            DoctorSpecializations = new List<DoctorSpecialization>
            {
                new()
                {
                    Specialization = new Specialization
                    {
                        SpecializationName = "Cardiology",
                        Services = new List<Service> { new() { Name = "ECG" } }
                    }
                }
            },
            Qualifications = new List<DoctorQualification>
            {
                new() { QualificationName = "MD", InstituteName = "Medical School" }
            }
        };

    private static Appointment CreateSampleAppointmentEntity() =>
        new()
        {
            Id = 1,
            PatientId = "patient1",
            DoctorId = "doctor1",
            DoctorName = "Dr Smith",
            AppointmentTakenDate = DateTime.UtcNow.Date,
            Amount = 30,
            PaymentStatus = "Pending",
            Patient = new Patient
            {
                Id = "patient1",
                UserName = "Jane Doe",
                Email = "jane@example.com",
                PasswordHash = "patient-hash"
            },
            Doctor = CreateSampleDoctorEntity(),
            AppointmentStatus = new AppointmentStatus { Status = AppointmentStatusEnum.Active }
        };
}
