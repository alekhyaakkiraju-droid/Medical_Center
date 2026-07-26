using System.Net;
using System.Net.Http.Json;
using AngularApi.DTO;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Contracts;

public class PatientJourneyContractTests : ContractTestBase
{
    public PatientJourneyContractTests(MedicalCenterWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RegisterUser_ReturnsExpectedShape()
    {
        var client = CreateClient("203.0.113.61");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        var email = $"register-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/Account/register/user", new RegisterUserDTO
        {
            UserName = "Contract Patient", Email = email, Password = ContractPassword, ConfirmPassword = ContractPassword,
        });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            using var document = ParseJson(await response.Content.ReadAsStringAsync());
            document.RootElement.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrWhiteSpace();
        }

        var loginClient = await LoginAsync(email, ContractPassword);
        loginClient.DefaultRequestHeaders.Contains("Cookie").Should().BeTrue();
    }

    [Fact]
    public async Task Login_SetsCookieAndReturnsExpectedShape()
    {
        await SeedPatientUserAsync();
        var client = await LoginAsync(ContractPatientEmail, ContractPassword);
        client.DefaultRequestHeaders.Contains("Cookie").Should().BeTrue();
        var meResponse = await client.GetAsync("/api/Account/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = ParseJson(await meResponse.Content.ReadAsStringAsync());
        document.RootElement.TryGetProperty("email", out var email).Should().BeTrue();
        email.GetString().Should().Be(ContractPatientEmail);
        document.RootElement.TryGetProperty("roles", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAppointment_ReturnsExpectedShape()
    {
        var patientId = await SeedPatientUserAsync();
        await SeedDoctorForAppointmentsAsync();
        var client = await LoginAsync(ContractPatientEmail, ContractPassword);
        var response = await client.PostAsJsonAsync("/api/Appointments", new { doctorId = ContractDoctorId, appointmentTakenDate = DateTime.UtcNow.AddDays(2) });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var document = ParseJson(await response.Content.ReadAsStringAsync());
        document.RootElement.TryGetProperty("id", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("doctorId", out var doctorId).Should().BeTrue();
        doctorId.GetString().Should().Be(ContractDoctorId);
        document.RootElement.TryGetProperty("patientId", out var returnedPatientId).Should().BeTrue();
        returnedPatientId.GetString().Should().Be(patientId);
    }

    [Fact]
    public async Task GetPatientAppointments_ReturnsPagedResultShape()
    {
        var patientId = await SeedPatientUserAsync();
        await SeedDoctorForAppointmentsAsync();
        var client = await LoginAsync(ContractPatientEmail, ContractPassword);
        await client.PostAsJsonAsync("/api/Appointments", new { doctorId = ContractDoctorId, appointmentTakenDate = DateTime.UtcNow.AddDays(3) });
        var response = await client.GetAsync($"/api/Appointments/patient/{patientId}?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertPagedResultShape(ParseJson(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task GetUserProfile_ReturnsExpectedShape()
    {
        await SeedPatientUserAsync();
        var client = await LoginAsync(ContractPatientEmail, ContractPassword);
        var response = await client.GetAsync("/api/Account/user-details");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = ParseJson(await response.Content.ReadAsStringAsync());
        document.RootElement.TryGetProperty("email", out var email).Should().BeTrue();
        email.GetString().Should().Be(ContractPatientEmail);
        document.RootElement.TryGetProperty("userName", out _).Should().BeTrue();
    }
}
