using AngularApi.Models;
using System.Net.Http.Headers;
using AngularApi.Contracts.Models;
using AngularApi.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Authorization;

public abstract class AuthorizationIntegrationTestBase : IClassFixture<MedicalCenterWebApplicationFactory>
{
    protected readonly MedicalCenterWebApplicationFactory Factory;

    protected AuthorizationIntegrationTestBase(MedicalCenterWebApplicationFactory factory)
    {
        Factory = factory;
    }

    protected HttpClient CreateAnonymousClient()
    {
        return Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    protected HttpClient CreateClientWithRole(string role)
        => CreateClientForUser("integration-test-user", role);

    protected HttpClient CreateClientForUser(string userId, params string[] roles)
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = Factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var token = TestJwtFactory.CreateTokenForUser(configuration, userId, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    protected async Task SeedPatientAsync(string patientId)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        if (!context.Patients.Any(p => p.Id == patientId))
        {
            context.Patients.Add(new Patient
            {
                Id = patientId,
                Name = "Test Patient",
                Email = $"{patientId}@example.com"
            });
            await context.SaveChangesAsync();
        }
    }

    protected async Task SeedDoctorAsync(string doctorId)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        if (!context.Doctors.Any(d => d.Id == doctorId))
        {
            context.Doctors.Add(new Doctor
            {
                Id = doctorId,
                Name = "Test Doctor"
            });
            await context.SaveChangesAsync();
        }
    }

    protected async Task<int> SeedAppointmentAsync(string patientId, string? doctorId = null)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DoctorName = doctorId ?? "Unassigned",
            AppointmentTakenDate = DateTime.UtcNow,
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }
}
