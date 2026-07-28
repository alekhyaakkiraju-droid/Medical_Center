using System.Net;
using System.Net.Http.Json;
using AngularApi.Contracts.Models;
using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Integration;

public class AppointmentConfirmationEmailIntegrationTests : IClassFixture<MailHogWebApplicationFactory>
{
    private readonly MailHogWebApplicationFactory _factory;

    public AppointmentConfirmationEmailIntegrationTests(MailHogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAppointment_SendsConfirmationEmailToMailHog()
    {
        if (!await MailHogTestHelper.IsAvailableAsync())
        {
            return;
        }

        await MailHogTestHelper.ClearMessagesAsync();

        string patientEmail;
        string patientPassword;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roleManager.EnsureRolesCreatedAsync();

            var registerDto = RegisterUserFixtures.Valid();
            patientEmail = registerDto.Email!;
            patientPassword = registerDto.Password!;
            var patient = new Patient
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                EmailConfirmed = true,
            };
            (await userManager.CreateAsync(patient, registerDto.Password!)).Succeeded.Should().BeTrue();
            await userManager.AddToRoleAsync(patient, "user");

            var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
            context.Doctors.Add(new Doctor { Id = "doctor-smoke", Name = "Dr. Smoke" });
            await context.SaveChangesAsync();
        }

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        client.DefaultRequestHeaders.Add("X-Test-Client-Ip", $"203.0.114.{Random.Shared.Next(1, 200)}");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/Account/login",
            new AngularApi.Contracts.DTO.LogInUserDTO
            {
                Email = patientEmail,
                Password = patientPassword,
            });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var appointmentDate = DateTime.UtcNow.AddDays(7);
        var createResponse = await client.PostAsJsonAsync(
            "/api/Appointments",
            new
            {
                doctorId = "doctor-smoke",
                medicalCenterId = 1,
                appointmentTakenDate = appointmentDate,
                probableStartTime = appointmentDate,
                name = "Smoke Patient",
                email = patientEmail,
                phone = "5551234567",
            });
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        var message = await MailHogTestHelper.WaitForMessageToAsync(patientEmail);
        message.Should().NotBeNull();
        message!.Content.Body.Should().Contain("Dr. Smoke");
        message.Content.Body.Should().Contain("Appointment Confirmation");
    }
}
