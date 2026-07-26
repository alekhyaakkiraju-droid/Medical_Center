using System.Net;
using System.Net.Http.Json;
using AngularApi.DTO;
using AngularApi.Tests.Authorization;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Controllers;

public class AdminControllerAuthorizationIntegrationTests : AuthorizationIntegrationTestBase
{
    public AdminControllerAuthorizationIntegrationTests(MedicalCenterWebApplicationFactory factory)
        : base(factory)
    {
    }

    private static BreachAssessmentDTO ValidAssessment() => new()
    {
        Description = "Suspicious failed login spike detected in audit logs",
        AffectedEntityTypes = ["Patient", "Appointment"],
        DiscoveryDate = DateTime.UtcNow.AddHours(-1),
        SeverityLevel = "High"
    };

    [Fact]
    public async Task PostBreachAssessment_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/admin/breach-assessment", ValidAssessment());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostBreachAssessment_WithUserRole_ReturnsForbidden()
    {
        var client = CreateClientWithRole("user");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/admin/breach-assessment", ValidAssessment());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostBreachAssessment_WithAdminRole_ReturnsOk()
    {
        var client = CreateClientWithRole("admin");
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/admin/breach-assessment", ValidAssessment());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BreachAssessmentResultDTO>();
        result.Should().NotBeNull();
        result!.AssessmentId.Should().NotBe(Guid.Empty);
        result.Status.Should().Be("Assessed");
    }
}
