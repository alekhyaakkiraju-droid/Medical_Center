using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AngularApi.Contracts.DTO;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Contracts;

public class AdminJourneyContractTests : ContractTestBase
{
    public AdminJourneyContractTests(MedicalCenterWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllAppointments_ReturnsPagedResultShape()
    {
        await SeedAdminUserAsync();
        var client = await LoginAsync(ContractAdminEmail, ContractPassword);
        var response = await client.GetAsync("/api/Appointments/GetAllAppointments?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertPagedResultShape(ParseJson(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task GetTotalEarnings_ReturnsExpectedShape()
    {
        await SeedAdminUserAsync();
        var client = await LoginAsync(ContractAdminEmail, ContractPassword);
        var response = await client.GetAsync("/api/Appointments/total-earnings");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = ParseJson(await response.Content.ReadAsStringAsync());
        document.RootElement.TryGetProperty("totalEarnings", out var totalEarnings).Should().BeTrue();
        totalEarnings.ValueKind.Should().BeOneOf(JsonValueKind.Number, JsonValueKind.String);
    }

    [Fact]
    public async Task GetAllPatients_ReturnsPagedResultShape()
    {
        await SeedAdminUserAsync();
        await SeedPatientUserAsync();
        var client = await LoginAsync(ContractAdminEmail, ContractPassword);
        var response = await client.GetAsync("/api/Patients?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertPagedResultShape(ParseJson(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task RegisterDoctor_ReturnsExpectedShape()
    {
        await SeedAdminUserAsync();
        var client = await LoginAsync(ContractAdminEmail, ContractPassword);
        var response = await client.GetAsync("/api/Patients?pageNumber=1&pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertPagedResultShape(ParseJson(await response.Content.ReadAsStringAsync()));
    }
}
