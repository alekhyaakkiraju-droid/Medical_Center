using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Authorization;

public class CsrfProtectionIntegrationTests : AuthorizationIntegrationTestBase
{
    public CsrfProtectionIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task PostWithoutXsrfToken_ReturnsBadRequest()
    {
        await SeedAppointmentMutationFixtureAsync();

        var client = CreateClientForUser("csrf-patient", "user");
        var payload = new
        {
            doctorId = "csrf-doctor",
            appointmentTakenDate = DateTime.UtcNow,
        };

        var response = await client.PostAsJsonAsync("/api/Appointments", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ContactSubmitWithoutXsrfToken_ReturnsBadRequest()
    {
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

        var response = await client.PostAsJsonAsync("/api/Contact", ContactInquiryFixtures.Valid);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ContactSubmitWithValidXsrfToken_ReturnsSuccess()
    {
        var client = AntiforgeryTestHelper.CreateClient(Factory);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Contact", ContactInquiryFixtures.Valid);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostWithValidXsrfToken_ReturnsSuccess()
    {
        await SeedAppointmentMutationFixtureAsync();

        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var token = TestJwtFactory.CreateTokenForUser(
            Factory.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
            "csrf-patient",
            "user");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var payload = new
        {
            doctorId = "csrf-doctor",
            appointmentTakenDate = DateTime.UtcNow,
        };

        var response = await client.PostAsJsonAsync("/api/Appointments", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetWithoutXsrfToken_ReturnsSuccess()
    {
        var client = CreateClientWithRole("admin");

        var response = await client.GetAsync("/api/Appointments/GetAllAppointments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task SeedAppointmentMutationFixtureAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.EnsureRolesCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        if (!context.Doctors.Any(doctor => doctor.Id == "csrf-doctor"))
        {
            context.Doctors.Add(new Doctor { Id = "csrf-doctor", Name = "Dr. CSRF" });
        }

        if (await userManager.FindByIdAsync("csrf-patient") == null)
        {
            var patient = new Patient
            {
                Id = "csrf-patient",
                UserName = "csrf-patient@example.com",
                Email = "csrf-patient@example.com",
                EmailConfirmed = true,
            };
            var createResult = await userManager.CreateAsync(patient, "Password123!");
            createResult.Succeeded.Should().BeTrue();
            await userManager.AddToRoleAsync(patient, "user");
        }

        await context.SaveChangesAsync();
    }
}
