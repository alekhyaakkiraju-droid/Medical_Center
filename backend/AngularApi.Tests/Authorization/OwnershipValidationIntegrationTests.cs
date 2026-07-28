using AngularApi.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Authorization;

public class OwnershipValidationIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public OwnershipValidationIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAppointment_WhenPatientOwnsAppointment_ReturnsSuccess()
    {
        var appointmentId = await SeedAppointmentAsync("patient-a", "doctor-1");

        var client = CreateAuthenticatedClient("patient-a", "user");
        var response = await client.GetAsync($"/api/Appointments/{appointmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAppointment_WhenPatientDoesNotOwnAppointment_ReturnsForbidden()
    {
        var appointmentId = await SeedAppointmentAsync("patient-a", "doctor-1");

        var client = CreateAuthenticatedClient("patient-b", "user");
        var response = await client.GetAsync($"/api/Appointments/{appointmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointment_WhenAssignedDoctor_ReturnsSuccess()
    {
        var appointmentId = await SeedAppointmentAsync("patient-a", "doctor-1");

        var client = CreateAuthenticatedClient("doctor-1", "doctor");
        var response = await client.GetAsync($"/api/Appointments/{appointmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAppointment_WhenAdmin_ReturnsSuccess()
    {
        var appointmentId = await SeedAppointmentAsync("patient-a", "doctor-1");

        var client = CreateAuthenticatedClient("admin-user", "admin");
        var response = await client.GetAsync($"/api/Appointments/{appointmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAppointment_WhenNonAssignedDoctor_ReturnsForbidden()
    {
        var appointmentId = await SeedAppointmentAsync("patient-a", "doctor-1");

        var client = CreateAuthenticatedClient("doctor-2", "doctor");
        var response = await client.GetAsync($"/api/Appointments/{appointmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentsByPatient_WhenPatientIdDoesNotMatchUser_ReturnsForbidden()
    {
        var client = CreateAuthenticatedClient("patient-a", "user");

        var response = await client.GetAsync("/api/Appointments/patient/patient-b");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDoctorBookings_WhenDoctorIdDoesNotMatchUser_ReturnsForbidden()
    {
        var client = CreateAuthenticatedClient("doctor-a", "doctor");

        var response = await client.GetAsync("/api/Doctors/doctor-b/bookings");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletePatientAppointment_WhenPatientIdDoesNotMatchUser_ReturnsForbidden()
    {
        var client = CreateAuthenticatedClient("patient-a", "user");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.DeleteAsync("/api/Patients/patient-b/appointments/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAppointmentsByPatient_AsAdminForAnyPatient_ReturnsSuccess()
    {
        await SeedAppointmentAsync("patient-b", "doctor-1");

        var client = CreateAuthenticatedClient("admin-user", "admin");

        var response = await client.GetAsync("/api/Appointments/patient/patient-b");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAppointmentsByPatient_WhenPatientIdMatchesUser_ReturnsSuccess()
    {
        await SeedAppointmentAsync("patient-a", "doctor-1");

        var client = CreateAuthenticatedClient("patient-a", "user");

        var response = await client.GetAsync("/api/Appointments/patient/patient-a");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PutPatientReview_AsAdminForAnyReview_ReturnsNoContent()
    {
        var reviewId = await SeedPatientReviewAsync("patient-b", "doctor-1");
        var client = CreateAuthenticatedClient("admin-user", "admin");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        var response = await client.PutAsJsonAsync($"/api/PatientReviews/{reviewId}", new UpdatePatientReviewDTO { DoctorId = "doctor-1", OverallRating = 5 });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PutMedicalCenter_AsAdmin_ReturnsNoContent()
    {
        var medicalCenterId = await SeedMedicalCenterAsync();
        var client = CreateAuthenticatedClient("admin-user", "admin");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        var response = await client.PutAsJsonAsync($"/api/MedicalCenters/{medicalCenterId}", new UpdateMedicalCenterDTO { HospitalAffiliationId = 1, TimeSlotPerClientInMin = 30, FirstConsultationFee = 100, FollowupConsultationFee = 75, StreetAddress = "123 Main St", City = "Austin", State = "TX", Zip = "78701" });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private HttpClient CreateAuthenticatedClient(string userId, params string[] roles)
    {
        var client = AntiforgeryTestHelper.CreateClient(_factory);

        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var token = TestJwtFactory.CreateTokenForUser(configuration, userId, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<int> SeedAppointmentAsync(string patientId, string doctorId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        if (!context.Patients.Any(p => p.Id == patientId))
        {
            context.Patients.Add(new Patient { Id = patientId, Name = "Test Patient", Email = "test@example.com" });
        }

        if (!context.Doctors.Any(d => d.Id == doctorId))
        {
            context.Doctors.Add(new Doctor { Id = doctorId, Name = "Test Doctor" });
        }

        var medicalCenterId = 1;
        if (!context.MedicalCenter.Any(m => m.Id == medicalCenterId))
        {
            context.MedicalCenter.Add(new MedicalCenter
            {
                Id = medicalCenterId,
                HospitalAffiliationId = 1,
                TimeSlotPerClientInMin = 30,
                FirstConsultationFee = 100,
                FollowupConsultationFee = 75,
                StreetAddress = "123 Main St",
                City = "Austin",
                State = "TX",
                Zip = "78701",
            });
        }

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DoctorName = "Test Doctor",
            MedicalCenterId = medicalCenterId,
            AppointmentTakenDate = DateTime.UtcNow,
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }

    private async Task<int> SeedPatientReviewAsync(string patientId, string doctorId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        if (!context.Patients.Any(p => p.Id == patientId)) context.Patients.Add(new Patient { Id = patientId, Name = "Review Patient", Email = "review@example.com" });
        if (!context.Doctors.Any(d => d.Id == doctorId)) context.Doctors.Add(new Doctor { Id = doctorId, Name = "Review Doctor" });
        var review = new PatientReview { PatientId = patientId, DoctorId = doctorId, OverallRating = 4, ReviewDate = DateTime.UtcNow };
        context.PatientReviews.Add(review);
        await context.SaveChangesAsync();
        return review.Id;
    }

    private async Task<int> SeedMedicalCenterAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var medicalCenter = new MedicalCenter { HospitalAffiliationId = 1, TimeSlotPerClientInMin = 30, FirstConsultationFee = 100, FollowupConsultationFee = 75, StreetAddress = "123 Main St", City = "Austin", State = "TX", Zip = "78701" };
        context.MedicalCenter.Add(medicalCenter);
        await context.SaveChangesAsync();
        return medicalCenter.Id;
    }
}
