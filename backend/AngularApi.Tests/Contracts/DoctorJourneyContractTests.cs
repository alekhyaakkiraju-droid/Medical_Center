using System.Net;
using System.Text.Json;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Contracts;

public class DoctorJourneyContractTests : ContractTestBase
{
    public DoctorJourneyContractTests(MedicalCenterWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDoctorBookings_ReturnsPagedResultShape()
    {
        var doctorId = await SeedDoctorUserAsync();
        var client = await LoginAsync(ContractDoctorEmail, ContractPassword);
        var response = await client.GetAsync($"/api/Doctors/{doctorId}/bookings?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertPagedResultShape(ParseJson(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task GetDoctorBookingsToday_ReturnsExpectedShape()
    {
        var doctorId = await SeedDoctorUserAsync();
        var client = await LoginAsync(ContractDoctorEmail, ContractPassword);
        var response = await client.GetAsync($"/api/Doctors/{doctorId}/bookings/today?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertPagedResultShape(ParseJson(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task GetDoctorReviews_ReturnsExpectedShape()
    {
        var doctorId = await SeedDoctorUserAsync();
        var client = await LoginAsync(ContractDoctorEmail, ContractPassword);
        var response = await client.GetAsync($"/api/Doctors/{doctorId}/reviews?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertPagedResultShape(ParseJson(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task GetDoctorRating_ReturnsExpectedShape()
    {
        var doctorId = await SeedDoctorUserAsync();
        var client = await LoginAsync(ContractDoctorEmail, ContractPassword);
        var response = await client.GetAsync($"/api/Doctors/{doctorId}/rating");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
        ParseJson(body).RootElement.ValueKind.Should().NotBe(JsonValueKind.Undefined);
    }
}
