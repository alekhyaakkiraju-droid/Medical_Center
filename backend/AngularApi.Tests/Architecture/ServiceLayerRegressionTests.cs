using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AngularApi.Models;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Architecture;

public class ServiceLayerRegressionTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private const string AdminUserId = "service-layer-regression-admin";

    private readonly MedicalCenterWebApplicationFactory _factory;

    public ServiceLayerRegressionTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AppointmentStatusController_SupportsFullCrudThroughPipeline()
    {
        var client = await CreateAdminClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/AppointmentStatus", new
        {
            Status = AppointmentStatusEnum.Active
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await ReadCreatedEntityIdAsync(createResponse);

        (await client.GetAsync($"/api/AppointmentStatus/{createdId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync($"/api/AppointmentStatus/{createdId}", new
        {
            Status = AppointmentStatusEnum.Complete
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.DeleteAsync($"/api/AppointmentStatus/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/AppointmentStatus/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SpecializationsController_SupportsFullCrudThroughPipeline()
    {
        var client = await CreateAdminClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/Specializations", new
        {
            SpecializationName = "Cardiology",
            Description = "Heart specialist",
            IsActive = true
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await ReadCreatedEntityIdAsync(createResponse);

        (await client.GetAsync($"/api/Specializations/{createdId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync($"/api/Specializations/{createdId}", new
        {
            SpecializationName = "Neurology",
            Description = "Brain specialist",
            IsActive = true
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.DeleteAsync($"/api/Specializations/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/Specializations/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MedicalCentersController_SupportsFullCrudThroughPipeline()
    {
        var client = await CreateAdminClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/MedicalCenters", new
        {
            StreetAddress = "100 Main St",
            City = "Boston",
            State = "MA",
            Zip = "02101"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await ReadCreatedEntityIdAsync(createResponse);

        (await client.GetAsync($"/api/MedicalCenters/{createdId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync($"/api/MedicalCenters/{createdId}", new
        {
            StreetAddress = "200 Main St",
            City = "Cambridge",
            State = "MA",
            Zip = "02139"
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.DeleteAsync($"/api/MedicalCenters/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/MedicalCenters/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MedicalCenterDoctorAvailabilitiesController_SupportsFullCrudThroughPipeline()
    {
        var medicalCenterId = await SeedMedicalCenterAsync();
        var client = await CreateAdminClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/MedicalCenterDoctorAvailabilities", new
        {
            MedicalCenterId = medicalCenterId,
            DayOfWeek = "Monday",
            StartTime = DateTime.Today.AddHours(9),
            EndTime = DateTime.Today.AddHours(17),
            IsAvailable = true
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await ReadCreatedEntityIdAsync(createResponse);

        (await client.GetAsync($"/api/MedicalCenterDoctorAvailabilities/{createdId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync($"/api/MedicalCenterDoctorAvailabilities/{createdId}", new
        {
            MedicalCenterId = medicalCenterId,
            DayOfWeek = "Tuesday",
            StartTime = DateTime.Today.AddHours(10),
            EndTime = DateTime.Today.AddHours(18),
            IsAvailable = false,
            ReasonOfUnavailability = "Holiday"
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.DeleteAsync($"/api/MedicalCenterDoctorAvailabilities/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/MedicalCenterDoctorAvailabilities/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PatientReviewsController_SupportsFullCrudThroughPipeline()
    {
        await SeedPatientAsync(AdminUserId);
        await SeedDoctorAsync("doctor-regression-1");
        var client = await CreateAdminClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/PatientReviews", new
        {
            DoctorId = "doctor-regression-1",
            OverallRating = 5,
            WaitTimeRating = 4,
            BedsideMannerRating = 5,
            Review = "Excellent care",
            IsDoctorRecommended = true
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await ReadCreatedEntityIdAsync(createResponse);

        (await client.GetAsync($"/api/PatientReviews/{createdId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync($"/api/PatientReviews/{createdId}", new
        {
            DoctorId = "doctor-regression-1",
            OverallRating = 4,
            WaitTimeRating = 4,
            BedsideMannerRating = 4,
            Review = "Updated review",
            IsDoctorRecommended = true
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.DeleteAsync($"/api/PatientReviews/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/PatientReviews/{createdId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var client = AntiforgeryTestHelper.CreateClient(_factory);

        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var token = TestJwtFactory.CreateTokenForUser(configuration, AdminUserId, "admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        return client;
    }

    private static async Task<int> ReadCreatedEntityIdAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.TryGetProperty("id", out var idElement).Should().BeTrue("created responses must include an id property");
        return idElement.GetInt32();
    }

    private async Task<int> SeedMedicalCenterAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var medicalCenter = new MedicalCenter
        {
            StreetAddress = "500 Clinic Rd",
            City = "Boston",
            State = "MA",
            Zip = "02108"
        };
        context.MedicalCenter.Add(medicalCenter);
        await context.SaveChangesAsync();
        return medicalCenter.Id;
    }

    private async Task SeedPatientAsync(string patientId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        if (!context.Patients.Any(p => p.Id == patientId))
        {
            context.Patients.Add(new Patient
            {
                Id = patientId,
                Name = "Regression Patient",
                Email = $"{patientId}@example.com"
            });
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedDoctorAsync(string doctorId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        if (!context.Doctors.Any(d => d.Id == doctorId))
        {
            context.Doctors.Add(new Doctor
            {
                Id = doctorId,
                Name = "Regression Doctor"
            });
            await context.SaveChangesAsync();
        }
    }
}
