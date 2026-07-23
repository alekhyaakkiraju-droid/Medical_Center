using System.Net;
using System.Net.Http.Json;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.Validators;

public class FluentValidationIntegrationTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public FluentValidationIntegrationTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ReturnsBadRequestWithValidationErrors()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var payload = new
        {
            UserName = "Jane Doe",
            Email = "not-an-email",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        var response = await client.PostAsJsonAsync("/api/Account/register/user", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email");
    }
}
