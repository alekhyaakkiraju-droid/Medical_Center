using System.Net;
using System.Net.Http.Headers;
using AngularApi.Models;
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

    private HttpClient CreateAuthenticatedClient(string userId, params string[] roles)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var token = TestJwtFactory.CreateTokenForUser(configuration, userId, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task SeedAppointmentAsync(string patientId, string doctorId)
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

        context.Appointments.Add(new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DoctorName = "Test Doctor",
            AppointmentTakenDate = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();
    }
}
