using AngularApi.Models;
using System.Net;
using System.Text.Json;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Authorization;

public class DoctorsControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public DoctorsControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDoctors_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Doctors");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDoctors_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");

        var response = await client.GetAsync("/api/Doctors");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDoctorBookings_WhenDoctorIdDoesNotMatchUser_ReturnsForbidden()
    {
        var client = CreateClientForUser("doctor-a", "doctor");

        var response = await client.GetAsync("/api/Doctors/doctor-b/bookings");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDoctorBookings_AsAdminForAnyDoctor_ReturnsSuccess()
    {
        await SeedDoctorAsync("doctor-b");

        var client = CreateClientForUser("admin-user", "admin");

        var response = await client.GetAsync("/api/Doctors/doctor-b/bookings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDoctor_ExistingDoctor_ReturnsDoctorDetailDtoWithoutSensitiveFields()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
            context.Doctors.Add(new Doctor
            {
                Id = "doctor-detail",
                Name = "Dr. Detail",
                PasswordHash = "must-not-leak",
                SecurityStamp = "must-not-leak",
                NormalizedEmail = "DR@EXAMPLE.COM",
                DoctorSpecializations = new List<DoctorSpecialization>
                {
                    new() { Specialization = new Specialization { SpecializationName = "Neurology" } }
                },
                Qualifications = new List<DoctorQualification>
                {
                    new() { QualificationName = "MD", InstituteName = "Yale" }
                }
            });
            await context.SaveChangesAsync();
        }

        var client = CreateClientForUser("doctor-detail", "doctor");
        var response = await client.GetAsync("/api/Doctors/doctor-detail");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var detail = JsonSerializer.Deserialize<DoctorDetailDTO>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        detail.Should().NotBeNull();
        detail!.Id.Should().Be("doctor-detail");
        detail.Name.Should().Be("Dr. Detail");
        detail.Specializations.Should().Contain("Neurology");
        detail.Qualifications.Should().ContainSingle(q => q.QualificationName == "MD");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.TryGetProperty("passwordHash", out _).Should().BeFalse();
        root.TryGetProperty("securityStamp", out _).Should().BeFalse();
        root.TryGetProperty("normalizedEmail", out _).Should().BeFalse();
        root.TryGetProperty("concurrencyStamp", out _).Should().BeFalse();
        root.TryGetProperty("createdAt", out _).Should().BeFalse();
        root.TryGetProperty("createdBy", out _).Should().BeFalse();
    }
}
